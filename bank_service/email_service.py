"""
Email Service — بيبعت كود التحقق (OTP) بإيميل حقيقي عن طريق SMTP.
البيانات بتتقرا من ملف .env (مش مكتوبة جوه الكود مباشرة لأسباب أمان).
"""

import os
import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

from dotenv import load_dotenv

load_dotenv()

SMTP_HOST = os.getenv("SMTP_HOST", "smtp.gmail.com")
SMTP_PORT = int(os.getenv("SMTP_PORT", "465"))
SMTP_EMAIL = os.getenv("SMTP_EMAIL")
SMTP_PASSWORD = os.getenv("SMTP_PASSWORD")


def send_otp_email(to_email: str, code: str) -> None:
    """
    بيبعت كود التحقق بإيميل حقيقي. لو الإعدادات مش مضبوطة في .env بيرمي خطأ واضح
    بدل ما يفشل بصمت — عشان تعرف فورًا إن فيه مشكلة إعدادات.
    """
    if not SMTP_EMAIL or not SMTP_PASSWORD:
        raise RuntimeError(
            "SMTP_EMAIL و SMTP_PASSWORD لازم يتضبطوا في ملف .env جوه bank_service/"
        )

    msg = MIMEMultipart("alternative")
    msg["Subject"] = "Pharaohs Legacy Bank — كود التحقق من الدفع"
    msg["From"] = SMTP_EMAIL
    msg["To"] = to_email

    text_body = f"كود التحقق الخاص بك هو: {code}\nصالح لمدة 5 دقائق فقط. لا تشاركه مع أي حد."

    html_body = f"""
    <div style="font-family:Tahoma,sans-serif;background:#0d0702;color:#f3e8d0;padding:28px;border-radius:10px;max-width:420px;margin:auto;">
        <h2 style="color:#c9a227;margin:0 0 16px;">𓋹 Pharaohs Legacy Bank</h2>
        <p style="margin:0 0 8px;">كود التحقق الخاص بك لإتمام عملية الدفع هو:</p>
        <p style="font-size:32px;font-weight:bold;letter-spacing:8px;color:#c9a227;margin:16px 0;">{code}</p>
        <p style="color:#a68a5c;font-size:.85rem;margin:0;">صالح لمدة 5 دقائق فقط. لا تشارك هذا الكود مع أي شخص.</p>
    </div>
    """

    msg.attach(MIMEText(text_body, "plain"))
    msg.attach(MIMEText(html_body, "html"))

    with smtplib.SMTP_SSL(SMTP_HOST, SMTP_PORT) as server:
        server.login(SMTP_EMAIL, SMTP_PASSWORD)
        server.sendmail(SMTP_EMAIL, to_email, msg.as_string())
