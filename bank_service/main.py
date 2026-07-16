"""
Pharaohs Legacy — Fake Bank Microservice
==========================================
Service مستقل بلغة Python (FastAPI) بيحاكي نظام بنكي حقيقي.
بيتكلم معاه مشروع ASP.NET Core الرئيسي عن طريق REST API عادي.

تشغيل السيرفر:
    uvicorn main:app --reload --port 8001

بعد التشغيل:
    - Swagger Docs:  http://127.0.0.1:8001/docs
    - Dashboard:      http://127.0.0.1:8001/dashboard
"""

from datetime import datetime
from typing import List

from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
from sqlalchemy.orm import Session

import models
import schemas
from email_service import send_otp_email

app = FastAPI(
    title="Pharaohs Legacy Bank",
    description="نظام بنكي وهمي (Fake Banking Microservice) لمشروع Pharaohs Legacy",
    version="1.0.0",
)

# السماح للموقع الأساسي (ASP.NET, بيشتغل غالبًا على بورت مختلف) يكلم الـ API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],       # في الإنتاج الحقيقي تحدد الدومين بتاع الموقع بس
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

models.init_db()


# =========================================================================
# Accounts
# =========================================================================

@app.post("/accounts/create", response_model=schemas.AccountCreatedResponse, tags=["Accounts"])
def create_account(payload: schemas.AccountCreate, db: Session = Depends(models.get_db)):
    """
    بيتنادى مرة واحدة بس — لحظة ما اليوزر يعمل Register في الموقع الرئيسي.
    ده الـ Endpoint الوحيد اللي بيرجع رقم الكارت كامل — زي أي بنك حقيقي بيوريك
    كارتك الجديد كامل أول مرة بس، وبعد كده أي استعلام هيرجعه Masked دايمًا.
    """
    existing = db.query(models.BankAccount).filter_by(user_email=payload.user_email).first()
    if existing:
        raise HTTPException(status_code=400, detail="الحساب موجود بالفعل لليوزر ده")

    account = models.BankAccount(
        user_email=payload.user_email,
        card_holder_name=payload.card_holder_name,
        card_number=models.generate_card_number(),
        expiry_date=f"{datetime.utcnow().month:02d}/{(datetime.utcnow().year + 4) % 100:02d}",
        cvv=models.generate_cvv(),
        balance=payload.initial_balance,
    )
    db.add(account)
    db.commit()
    db.refresh(account)

    # سجل أول Transaction (فتح الحساب)
    if payload.initial_balance > 0:
        tx = models.BankTransaction(
            account_id=account.id,
            type="TopUp",
            amount=payload.initial_balance,
            related_type="AccountOpening",
            related_id=None,
            balance_after=account.balance,
            note="رصيد ابتدائي عند فتح الحساب",
        )
        db.add(tx)
        db.commit()

    return schemas.AccountCreatedResponse(
        user_email=account.user_email,
        card_holder_name=account.card_holder_name,
        card_number=account.card_number,
        expiry_date=account.expiry_date,
        cvv=account.cvv,
        balance=account.balance,
        created_at=account.created_at,
    )


@app.get("/accounts/{user_email}", response_model=schemas.AccountResponse, tags=["Accounts"])
def get_account(user_email: str, db: Session = Depends(models.get_db)):
    account = _get_account_or_404(db, user_email)
    return _to_account_response(account)


@app.get(
    "/accounts/{user_email}/transactions",
    response_model=List[schemas.TransactionResponse],
    tags=["Accounts"],
)
def get_transactions(user_email: str, db: Session = Depends(models.get_db)):
    account = _get_account_or_404(db, user_email)
    return (
        db.query(models.BankTransaction)
        .filter_by(account_id=account.id)
        .order_by(models.BankTransaction.created_at.desc())
        .all()
    )


# =========================================================================
# Top Up (شحن رصيد)
# =========================================================================

@app.post(
    "/accounts/{user_email}/topup",
    response_model=schemas.TransactionResponse,
    tags=["Transactions"],
)
def top_up(user_email: str, payload: schemas.TopUpRequest, db: Session = Depends(models.get_db)):
    account = _get_account_or_404(db, user_email)

    account.balance += payload.amount
    tx = models.BankTransaction(
        account_id=account.id,
        type="TopUp",
        amount=payload.amount,
        related_type="TopUp",
        balance_after=account.balance,
        status="Success",
    )
    db.add(tx)
    db.commit()
    db.refresh(tx)
    return tx


# =========================================================================
# Charge (خصم — حجز أو شراء من المتجر)
# =========================================================================

@app.post(
    "/accounts/{user_email}/charge",
    response_model=schemas.ChargeResponse,
    tags=["Transactions"],
)
def charge(user_email: str, payload: schemas.ChargeRequest, db: Session = Depends(models.get_db)):
    account = _get_account_or_404(db, user_email)

    final_amount = payload.amount
    discount_applied = 0.0
    coupon = None

    # لو فيه كود خصم اتبعت
    if payload.coupon_code:
        coupon = db.query(models.Coupon).filter_by(
            code=payload.coupon_code, user_email=user_email
        ).first()
        if not coupon:
            raise HTTPException(status_code=404, detail="كود الخصم غير موجود لهذا المستخدم")
        if coupon.is_used:
            raise HTTPException(status_code=400, detail="الكود ده اتستخدم قبل كده")
        if coupon.expires_at < datetime.utcnow():
            raise HTTPException(status_code=400, detail="الكود ده منتهي الصلاحية")

        discount_applied = round(payload.amount * (coupon.discount_percent / 100), 2)
        final_amount = round(payload.amount - discount_applied, 2)

    if account.balance < final_amount:
        # نسجل محاولة فاشلة عشان تبان في الداشبورد
        tx = models.BankTransaction(
            account_id=account.id,
            type="Purchase",
            amount=final_amount,
            related_type=payload.related_type,
            related_id=payload.related_id,
            balance_after=account.balance,
            status="Failed",
            note="رصيد غير كافٍ",
        )
        db.add(tx)
        db.commit()
        return schemas.ChargeResponse(
            success=False,
            message="رصيد غير كافٍ لإتمام العملية",
            original_amount=payload.amount,
            discount_applied=discount_applied,
            final_amount=final_amount,
            balance_after=account.balance,
        )

    # الخصم الفعلي
    account.balance -= final_amount
    tx = models.BankTransaction(
        account_id=account.id,
        type="Purchase",
        amount=final_amount,
        related_type=payload.related_type,
        related_id=payload.related_id,
        balance_after=account.balance,
        status="Success",
        note=payload.note,
    )
    db.add(tx)

    if coupon:
        coupon.is_used = True
        coupon.used_at = datetime.utcnow()
        coupon.used_on_type = payload.related_type
        coupon.used_on_id = payload.related_id

    db.commit()

    return schemas.ChargeResponse(
        success=True,
        message="تمت العملية بنجاح",
        original_amount=payload.amount,
        discount_applied=discount_applied,
        final_amount=final_amount,
        balance_after=account.balance,
    )


# =========================================================================
# OTP (كود تحقق قبل إتمام أي عملية دفع بكارت)
# =========================================================================

@app.post(
    "/payments/request-otp",
    response_model=schemas.OtpRequestResponse,
    tags=["Payments"],
)
def request_otp(payload: schemas.OtpRequest, db: Session = Depends(models.get_db)):
    """
    بيتنادى قبل /payments/charge — بيتأكد إن فيه حساب بنكي فعلاً، بعدين يولّد
    كود 6 أرقام ويبعته بإيميل حقيقي. الكود صالح 5 دقايق ومربوط بنفس الـ
    related_type + related_id عشان محدش يستخدم كود قديم لعملية تانية.
    """
    _get_account_or_404(db, payload.user_email)  # يتأكد إن العميل موجود قبل ما يبعت كود

    code = models.OtpCode.generate_code()
    otp = models.OtpCode(
        user_email=payload.user_email,
        code=code,
        purpose="Payment",
        related_type=payload.related_type,
        related_id=payload.related_id,
        expires_at=models.OtpCode.default_expiry(5),
    )
    db.add(otp)
    db.commit()

    try:
        send_otp_email(payload.user_email, code)
    except Exception:
        # لو الإيميل فشل يتبعت، نلغي الكود عشان مايفضلش صالح من غير ما اليوزر يستلمه
        db.delete(otp)
        db.commit()
        raise HTTPException(
            status_code=502,
            detail="تعذر إرسال كود التحقق، حاول مرة أخرى",
        )

    return schemas.OtpRequestResponse(
        success=True,
        message="تم إرسال كود التحقق إلى بريدك الإلكتروني",
        expires_in_seconds=300,
    )


# =========================================================================
# Card-Validated Payment (زي أي Payment Gateway حقيقي — Stripe/PayPal)
# =========================================================================

@app.post(
    "/payments/charge",
    response_model=schemas.ChargeResponse,
    tags=["Payments"],
)
def charge_by_card(payload: schemas.CardChargeRequest, db: Session = Depends(models.get_db)):
    """
    ده الـ Endpoint اللي بيتنادى من صفحة الحجز/الدفع في الموقع.
    مش بيدفع بمجرد الإيميل — لازم بيانات الكارت (رقم/اسم/تاريخ/CVV) تتطابق
    فعليًا مع حساب موجود في البنك، وإلا العملية بترفض. بالظبط زي أي بوابة دفع حقيقية.
    """
    account = db.query(models.BankAccount).filter_by(user_email=payload.user_email).first()
    if not account:
        raise HTTPException(
            status_code=404,
            detail="لا يوجد حساب بنكي مرتبط بهذا البريد الإلكتروني — يجب أن تكون عميلاً في البنك لإتمام الحجز",
        )

    # تحقق شامل من كل بيانات الكارت — رسالة واحدة عامة لو أي حاجة غلط (زي البوابات الحقيقية بالظبط،
    # عشان محدش يقدر "يجرب" ويعرف مين الحقل الغلط بالظبط)
    card_matches = (
        account.card_number == payload.card_number
        and account.cvv == payload.cvv
        and account.expiry_date == payload.expiry_date
        and account.card_holder_name.strip().lower() == payload.card_holder_name.strip().lower()
    )
    if not card_matches:
        # نسجل محاولة فاشلة في اللوج عشان تبان في الداشبورد (بدون تفاصيل حساسة)
        tx = models.BankTransaction(
            account_id=account.id,
            type="Purchase",
            amount=payload.amount,
            related_type=payload.related_type,
            related_id=payload.related_id,
            balance_after=account.balance,
            status="Failed",
            note="بيانات الدفع غير صحيحة",
        )
        db.add(tx)
        db.commit()
        raise HTTPException(status_code=400, detail="بيانات الدفع غير صحيحة")

    # تحقق من كود الـ OTP — لازم يكون آخر كود اتبعت لنفس العملية بالظبط، لسه صالح، ومستخدمش قبل كده
    otp = (
        db.query(models.OtpCode)
        .filter_by(
            user_email=payload.user_email,
            related_type=payload.related_type,
            related_id=payload.related_id,
            purpose="Payment",
            is_used=False,
        )
        .order_by(models.OtpCode.created_at.desc())
        .first()
    )

    otp_valid = (
        otp is not None
        and otp.attempts < 3
        and otp.expires_at >= datetime.utcnow()
        and otp.code == payload.otp_code
    )

    if otp is not None and otp.code != payload.otp_code:
        otp.attempts += 1
        db.commit()

    if not otp_valid:
        tx = models.BankTransaction(
            account_id=account.id,
            type="Purchase",
            amount=payload.amount,
            related_type=payload.related_type,
            related_id=payload.related_id,
            balance_after=account.balance,
            status="Failed",
            note="كود تحقق غير صحيح أو منتهي",
        )
        db.add(tx)
        db.commit()
        raise HTTPException(status_code=400, detail="كود التحقق غير صحيح أو منتهي الصلاحية")

    otp.is_used = True
    db.commit()

    # لو الكارت والكود مطابقين، نفس منطق الخصم العادي (كوبون + رصيد)
    final_amount = payload.amount
    discount_applied = 0.0
    coupon = None

    if payload.coupon_code:
        coupon = db.query(models.Coupon).filter_by(
            code=payload.coupon_code, user_email=payload.user_email
        ).first()
        if not coupon:
            raise HTTPException(status_code=404, detail="كود الخصم غير موجود لهذا المستخدم")
        if coupon.is_used:
            raise HTTPException(status_code=400, detail="الكود ده اتستخدم قبل كده")
        if coupon.expires_at < datetime.utcnow():
            raise HTTPException(status_code=400, detail="الكود ده منتهي الصلاحية")

        discount_applied = round(payload.amount * (coupon.discount_percent / 100), 2)
        final_amount = round(payload.amount - discount_applied, 2)

    if account.balance < final_amount:
        tx = models.BankTransaction(
            account_id=account.id,
            type="Purchase",
            amount=final_amount,
            related_type=payload.related_type,
            related_id=payload.related_id,
            balance_after=account.balance,
            status="Failed",
            note="رصيد غير كافٍ",
        )
        db.add(tx)
        db.commit()
        return schemas.ChargeResponse(
            success=False,
            message="رصيد غير كافٍ لإتمام العملية",
            original_amount=payload.amount,
            discount_applied=discount_applied,
            final_amount=final_amount,
            balance_after=account.balance,
        )

    account.balance -= final_amount
    tx = models.BankTransaction(
        account_id=account.id,
        type="Purchase",
        amount=final_amount,
        related_type=payload.related_type,
        related_id=payload.related_id,
        balance_after=account.balance,
        status="Success",
        note=payload.note,
    )
    db.add(tx)

    if coupon:
        coupon.is_used = True
        coupon.used_at = datetime.utcnow()
        coupon.used_on_type = payload.related_type
        coupon.used_on_id = payload.related_id

    db.commit()

    return schemas.ChargeResponse(
        success=True,
        message="تمت العملية بنجاح",
        original_amount=payload.amount,
        discount_applied=discount_applied,
        final_amount=final_amount,
        balance_after=account.balance,
    )


@app.post(
    "/payments/refund",
    response_model=schemas.RefundResponse,
    tags=["Payments"],
)
def refund(payload: schemas.RefundRequest, db: Session = Depends(models.get_db)):
    """
    بيتنادى لما اليوزر يلغي حجز (جوه الـ 48 ساعة زي قاعدة الإلغاء الموجودة في الموقع).
    المبلغ بيرجع فورًا وكامل للرصيد. لو مبعتش amount، البنك بيدور على آخر عملية
    Purchase ناجحة بنفس related_type + related_id ويرجع نفس قيمتها بالظبط.
    """
    account = _get_account_or_404(db, payload.user_email)

    # 🆕 منع الاسترجاع المزدوج: لو العملية دي (نفس related_type + related_id)
    # اتعمل لها Refund ناجح قبل كده، نرفض فورًا. بدون التحقق ده كان ممكن حد
    # (الأدمن أو حتى نداء متكرر بالغلط) يرجّع نفس المبلغ أكتر من مرة.
    existing_refund = (
        db.query(models.BankTransaction)
        .filter_by(
            account_id=account.id,
            related_type=payload.related_type,
            related_id=payload.related_id,
            type="Refund",
            status="Success",
        )
        .first()
    )
    if existing_refund:
        raise HTTPException(
            status_code=400,
            detail="This transaction has already been refunded — it cannot be refunded again",
        )

    refund_amount = payload.amount
    if refund_amount is None:
        original_tx = (
            db.query(models.BankTransaction)
            .filter_by(
                account_id=account.id,
                related_type=payload.related_type,
                related_id=payload.related_id,
                type="Purchase",
                status="Success",
            )
            .order_by(models.BankTransaction.created_at.desc())
            .first()
        )
        if not original_tx:
            raise HTTPException(
                status_code=404,
                detail="No original payment found for this booking to refund",
            )
        refund_amount = original_tx.amount

    account.balance += refund_amount
    tx = models.BankTransaction(
        account_id=account.id,
        type="Refund",
        amount=refund_amount,
        related_type=payload.related_type,
        related_id=payload.related_id,
        balance_after=account.balance,
        status="Success",
        note="استرجاع كامل بسبب إلغاء الحجز",
    )
    db.add(tx)
    db.commit()

    return schemas.RefundResponse(
        success=True,
        message="تم استرجاع المبلغ بالكامل إلى رصيدك",
        refunded_amount=refund_amount,
        balance_after=account.balance,
    )


# =========================================================================
# Coupons (أكواد الخصم من الكويز)
# =========================================================================

@app.post("/coupons/create", response_model=schemas.CouponResponse, tags=["Coupons"])
def create_coupon(payload: schemas.CouponCreate, db: Session = Depends(models.get_db)):
    """بتتنادى من الموقع الرئيسي لما اليوزر يجيب Score كويس في الكويز"""
    code = models.Coupon.generate_code()
    coupon = models.Coupon(
        code=code,
        user_email=payload.user_email,
        discount_percent=payload.discount_percent,
        source_type=payload.source_type,
        expires_at=models.Coupon.default_expiry(payload.valid_days),
    )
    db.add(coupon)
    db.commit()
    db.refresh(coupon)
    return coupon


@app.get("/coupons/{user_email}", response_model=List[schemas.CouponResponse], tags=["Coupons"])
def list_user_coupons(user_email: str, db: Session = Depends(models.get_db)):
    return (
        db.query(models.Coupon)
        .filter_by(user_email=user_email)
        .order_by(models.Coupon.created_at.desc())
        .all()
    )


@app.post("/coupons/validate", tags=["Coupons"])
def validate_coupon(payload: schemas.CouponValidateRequest, db: Session = Depends(models.get_db)):
    """بيتنادى وقت ما اليوزر يحط الكود في صفحة الحجز/المتجر — بس عشان يتأكد إنه شغال (من غير ما يستخدمه فعليًا)"""
    coupon = db.query(models.Coupon).filter_by(
        code=payload.code, user_email=payload.user_email
    ).first()
    if not coupon:
        raise HTTPException(status_code=404, detail="كود غير موجود")
    if coupon.is_used:
        raise HTTPException(status_code=400, detail="الكود مستخدم بالفعل")
    if coupon.expires_at < datetime.utcnow():
        raise HTTPException(status_code=400, detail="الكود منتهي الصلاحية")

    return {"valid": True, "discount_percent": coupon.discount_percent, "expires_at": coupon.expires_at}


# =========================================================================
# Dashboard (المتابعة)
# =========================================================================

@app.get("/dashboard", tags=["Dashboard"], include_in_schema=False)
def dashboard():
    return FileResponse("static/dashboard.html")


@app.get("/api/dashboard/summary", tags=["Dashboard"])
def dashboard_summary(db: Session = Depends(models.get_db)):
    accounts = db.query(models.BankAccount).all()
    total_balance = sum(a.balance for a in accounts)
    total_accounts = len(accounts)
    total_transactions = db.query(models.BankTransaction).count()
    active_coupons = db.query(models.Coupon).filter_by(is_used=False).count()

    return {
        "total_accounts": total_accounts,
        "total_balance": round(total_balance, 2),
        "total_transactions": total_transactions,
        "active_coupons": active_coupons,
    }


@app.get("/api/dashboard/accounts", tags=["Dashboard"])
def dashboard_accounts(db: Session = Depends(models.get_db)):
    accounts = db.query(models.BankAccount).order_by(models.BankAccount.created_at.desc()).all()
    return [_to_account_response(a) for a in accounts]


@app.get("/api/dashboard/transactions", tags=["Dashboard"])
def dashboard_transactions(db: Session = Depends(models.get_db)):
    txs = (
        db.query(models.BankTransaction)
        .order_by(models.BankTransaction.created_at.desc())
        .limit(200)
        .all()
    )
    result = []
    for tx in txs:
        result.append({
            "id": tx.id,
            "user_email": tx.account.user_email if tx.account else None,
            "type": tx.type,
            "amount": tx.amount,
            "related_type": tx.related_type,
            "related_id": tx.related_id,
            "balance_after": tx.balance_after,
            "status": tx.status,
            "note": tx.note,
            "created_at": tx.created_at,
        })
    return result


@app.get("/api/dashboard/coupons", tags=["Dashboard"])
def dashboard_coupons(db: Session = Depends(models.get_db)):
    coupons = db.query(models.Coupon).order_by(models.Coupon.created_at.desc()).all()
    return [
        {
            "code": c.code,
            "user_email": c.user_email,
            "discount_percent": c.discount_percent,
            "is_used": c.is_used,
            "expires_at": c.expires_at,
            "created_at": c.created_at,
        }
        for c in coupons
    ]


# =========================================================================
# Delete Account
# =========================================================================

@app.delete("/accounts/{user_email}", tags=["Accounts"])
def delete_account(user_email: str, db: Session = Depends(models.get_db)):
    account = db.query(models.BankAccount).filter_by(user_email=user_email).first()

    if not account:
        raise HTTPException(status_code=404, detail="الحساب غير موجود")

    db.delete(account)
    db.commit()

    return {"message": "تم حذف الحساب بنجاح"}





# =========================================================================
# Helpers
# =========================================================================

def _get_account_or_404(db: Session, user_email: str) -> models.BankAccount:
    account = db.query(models.BankAccount).filter_by(user_email=user_email).first()
    if not account:
        raise HTTPException(status_code=404, detail="لا يوجد حساب بنكي لهذا المستخدم")
    return account


def _to_account_response(account: models.BankAccount) -> schemas.AccountResponse:
    return schemas.AccountResponse(
        user_email=account.user_email,
        card_holder_name=account.card_holder_name,
        masked_card_number=models.mask_card_number(account.card_number),
        expiry_date=account.expiry_date,
        balance=account.balance,
        is_active=account.is_active,
        created_at=account.created_at,
    )
