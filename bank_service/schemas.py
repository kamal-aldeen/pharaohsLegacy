"""
Pydantic Schemas — بيحددوا شكل الـ Request/Response لكل Endpoint
"""

from datetime import datetime
from typing import Optional
from pydantic import BaseModel, EmailStr, Field


# ---------------------------- Accounts ----------------------------

class AccountCreate(BaseModel):
    user_email: EmailStr
    card_holder_name: str
    initial_balance: float = Field(default=1000.0, ge=0)


class AccountResponse(BaseModel):
    user_email: str
    card_holder_name: str
    masked_card_number: str
    expiry_date: str
    balance: float
    is_active: bool
    created_at: datetime

    class Config:
        from_attributes = True


class AccountCreatedResponse(BaseModel):
    """
    زي أي بنك حقيقي بيوريك رقم كارتك كامل مرة واحدة بس وقت الإصدار —
    بعد كده كل حاجة بترجع Masked. الـ Response ده بيتستخدم في /accounts/create بس.
    """
    user_email: str
    card_holder_name: str
    card_number: str
    expiry_date: str
    cvv: str
    balance: float
    created_at: datetime

    class Config:
        from_attributes = True


# ---------------------------- Transactions ----------------------------

class TopUpRequest(BaseModel):
    amount: float = Field(gt=0, le=50000, description="أقصى شحنة 50,000 وهمي في المرة الواحدة")


class ChargeRequest(BaseModel):
    amount: float = Field(gt=0)
    related_type: str = Field(description="Booking / ShopOrder")
    related_id: str
    coupon_code: Optional[str] = None
    note: Optional[str] = None


class CardChargeRequest(BaseModel):
    """
    الدفع الحقيقي بتاع الحجز/المتجر — زي أي Payment Gateway حقيقي:
    اليوزر بيدخل بيانات كارت، والبنك بيتحقق إنها مطابقة فعليًا للحساب المسجل عنده.
    """
    user_email: EmailStr
    card_number: str = Field(min_length=16, max_length=16)
    card_holder_name: str
    expiry_date: str = Field(description="MM/YY")
    cvv: str = Field(min_length=3, max_length=3)
    otp_code: str = Field(min_length=6, max_length=6, description="كود التحقق اللي اتبعت بالإيميل")
    amount: float = Field(gt=0)
    related_type: str = Field(description="Booking / ShopOrder")
    related_id: str
    coupon_code: Optional[str] = None
    note: Optional[str] = None


class RefundRequest(BaseModel):
    user_email: EmailStr
    related_type: str = Field(description="Booking / ShopOrder")
    related_id: str
    amount: Optional[float] = Field(
        default=None, gt=0,
        description="لو مش متبعوت، البنك بيرجع نفس المبلغ اللي اتخصم أصلاً على نفس related_id"
    )


class RefundResponse(BaseModel):
    success: bool
    message: str
    refunded_amount: float
    balance_after: float


class ChargeResponse(BaseModel):
    success: bool
    message: str
    original_amount: float
    discount_applied: float = 0
    final_amount: float
    balance_after: float


class TransactionResponse(BaseModel):
    id: int
    type: str
    amount: float
    related_type: Optional[str]
    related_id: Optional[str]
    balance_after: float
    status: str
    note: Optional[str]
    created_at: datetime

    class Config:
        from_attributes = True


# ---------------------------- OTP (Payment Verification) ----------------------------

class OtpRequest(BaseModel):
    user_email: EmailStr
    related_type: str = Field(description="Booking / ShopOrder")
    related_id: str


class OtpRequestResponse(BaseModel):
    success: bool
    message: str
    expires_in_seconds: int


# ---------------------------- Coupons ----------------------------

class CouponCreate(BaseModel):
    user_email: EmailStr
    discount_percent: float = Field(default=20.0, gt=0, le=100)
    source_type: str = "Quiz"
    valid_days: int = Field(default=10, gt=0)


class CouponResponse(BaseModel):
    code: str
    user_email: str
    discount_percent: float
    is_used: bool
    expires_at: datetime
    created_at: datetime

    class Config:
        from_attributes = True


class CouponValidateRequest(BaseModel):
    code: str
    user_email: EmailStr
