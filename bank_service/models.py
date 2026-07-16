"""
Pharaohs Legacy Bank - Database Models
========================================
نظام بنكي وهمي (Fake Banking System) بيحاكي نظام بنكي حقيقي:
- BankAccount: حساب بنكي لكل يوزر (كارت + رصيد)
- BankTransaction: سجل كل عملية (شحن / خصم / استرجاع)
- Coupon: أكواد الخصم اللي بتطلع من الكويز
"""

import random
import string
from datetime import datetime, timedelta

from sqlalchemy import (
    Column, Integer, String, Float, Boolean, DateTime, ForeignKey, create_engine
)
from sqlalchemy.orm import declarative_base, relationship, sessionmaker

Base = declarative_base()


def generate_card_number() -> str:
    """رقم كارت وهمي 16 رقم، شكله زي فيزا حقيقية (بيبدأ بـ 4)"""
    return "4" + "".join(random.choices(string.digits, k=15))


def generate_cvv() -> str:
    return "".join(random.choices(string.digits, k=3))


def mask_card_number(card_number: str) -> str:
    """بيرجع الشكل المعروض بس: **** **** **** 1234"""
    return f"**** **** **** {card_number[-4:]}"


class BankAccount(Base):
    __tablename__ = "bank_accounts"

    id = Column(Integer, primary_key=True, index=True)
    user_email = Column(String, unique=True, index=True, nullable=False)
    card_holder_name = Column(String, nullable=False)
    card_number = Column(String, nullable=False)          # مخزن كامل (نظام وهمي - في الواقع الحقيقي بيتشفر)
    expiry_date = Column(String, nullable=False)           # MM/YY
    cvv = Column(String, nullable=False)
    balance = Column(Float, default=0.0, nullable=False)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, default=datetime.utcnow)

    transactions = relationship(
        "BankTransaction", back_populates="account", cascade="all, delete-orphan"
    )


class BankTransaction(Base):
    __tablename__ = "bank_transactions"

    id = Column(Integer, primary_key=True, index=True)
    account_id = Column(Integer, ForeignKey("bank_accounts.id"), nullable=False)

    type = Column(String, nullable=False)          # TopUp / Purchase / Refund
    amount = Column(Float, nullable=False)
    related_type = Column(String, nullable=True)   # Booking / ShopOrder / TopUp
    related_id = Column(String, nullable=True)
    balance_after = Column(Float, nullable=False)
    status = Column(String, default="Success")     # Success / Failed
    note = Column(String, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow)

    account = relationship("BankAccount", back_populates="transactions")


class Coupon(Base):
    __tablename__ = "coupons"

    id = Column(Integer, primary_key=True, index=True)
    code = Column(String, unique=True, index=True, nullable=False)
    user_email = Column(String, index=True, nullable=False)
    discount_percent = Column(Float, nullable=False)
    source_type = Column(String, default="Quiz")
    is_used = Column(Boolean, default=False)
    used_at = Column(DateTime, nullable=True)
    used_on_type = Column(String, nullable=True)   # Booking / ShopOrder
    used_on_id = Column(String, nullable=True)
    expires_at = Column(DateTime, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)

    @staticmethod
    def generate_code(prefix: str = "QUIZ") -> str:
        suffix = "".join(random.choices(string.ascii_uppercase + string.digits, k=6))
        return f"{prefix}-{suffix}"

    @staticmethod
    def default_expiry(days: int = 10) -> datetime:
        return datetime.utcnow() + timedelta(days=days)


class OtpCode(Base):
    __tablename__ = "otp_codes"

    id = Column(Integer, primary_key=True, index=True)
    user_email = Column(String, index=True, nullable=False)
    code = Column(String, nullable=False)
    purpose = Column(String, default="Payment")     # مستقبلاً ممكن يتستخدم لأغراض تانية
    related_type = Column(String, nullable=True)    # Booking / ShopOrder
    related_id = Column(String, nullable=True)
    is_used = Column(Boolean, default=False)
    attempts = Column(Integer, default=0)           # لمنع تجربة الكود أكتر من 3 مرات
    expires_at = Column(DateTime, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)

    @staticmethod
    def generate_code() -> str:
        return "".join(random.choices(string.digits, k=6))

    @staticmethod
    def default_expiry(minutes: int = 5) -> datetime:
        return datetime.utcnow() + timedelta(minutes=minutes)


# ---------------------------------------------------------------------------
# Engine / Session setup — SQLite مستقلة تمامًا عن الـ SQL Server بتاع الموقع
# ---------------------------------------------------------------------------
DATABASE_URL = "sqlite:///./bank.db"
engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False, "timeout": 15})
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def init_db():
    Base.metadata.create_all(bind=engine)
