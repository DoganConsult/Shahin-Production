#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Create Tenant 'Test 1' with Admin user and send welcome email
"""
import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

import psycopg2
import uuid
import hashlib
import secrets
import base64
import struct
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from datetime import datetime, timezone

# Database connection settings
DB_CONFIG = {
    'host': 'caboose.proxy.rlwy.net',
    'port': 11527,
    'database': 'GrcMvcDb',
    'user': 'postgres',
    'password': 'QNcTvViWopMfCunsyIkkXwuDpufzhkLs',
    'sslmode': 'require'
}

# Tenant and Admin details
TENANT_NAME = "Test 1"
TENANT_SLUG = "test-1"
TENANT_CODE = "TEST1"
ADMIN_EMAIL = "elgazzar082@gmail.com"
ADMIN_PASSWORD = "Admin@Test123!"  # Temporary password - must be changed on first login
ADMIN_FIRST_NAME = "Admin"
ADMIN_LAST_NAME = "Test1"

# SMTP Configuration
SMTP_CONFIG = {
    'host': 'smtp.office365.com',
    'port': 587,
    'username': 'Info@doganconsult.com',
    'password': 'AhmEma$123456',
    'from_email': 'Info@doganconsult.com'
}

LOGIN_URL = "https://shahin-ai.com/login"


def send_welcome_email(tenant_name: str, admin_email: str, password: str) -> bool:
    """Send welcome email to the tenant admin with credentials."""
    try:
        print(f"\nSending welcome email to {admin_email}...")

        # Create message
        msg = MIMEMultipart('alternative')
        msg['Subject'] = f"Welcome to Shahin AI GRC Platform - Your Admin Credentials"
        msg['From'] = SMTP_CONFIG['from_email']
        msg['To'] = admin_email

        # Plain text version
        text_content = f"""
Dear Admin,

Your tenant "{tenant_name}" has been created on Shahin AI GRC Platform.

LOGIN CREDENTIALS:
- Email: {admin_email}
- Temporary Password: {password}
- Login URL: {LOGIN_URL}

IMPORTANT: You must change your password on first login.

GETTING STARTED:
1. Log in with your credentials above
2. Complete the onboarding wizard
3. Set up your organization profile
4. Invite team members

Need help? Contact support@shahin-ai.com

Best regards,
Shahin AI Team
"""

        # HTML version
        html_content = f"""
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .credentials {{ background: #fff; border: 2px solid #667eea; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .credentials h3 {{ color: #667eea; margin-top: 0; }}
        .credential-item {{ margin: 10px 0; }}
        .credential-label {{ font-weight: bold; color: #555; }}
        .credential-value {{ color: #333; font-family: monospace; background: #f0f0f0; padding: 5px 10px; border-radius: 4px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .steps {{ background: #fff; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .steps ol {{ margin: 0; padding-left: 20px; }}
        .steps li {{ margin: 10px 0; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Welcome to Shahin AI</h1>
            <p>GRC Platform</p>
        </div>
        <div class="content">
            <p>Dear Admin,</p>
            <p>Your tenant <strong>"{tenant_name}"</strong> has been successfully created on Shahin AI GRC Platform.</p>

            <div class="credentials">
                <h3>Your Login Credentials</h3>
                <div class="credential-item">
                    <span class="credential-label">Email:</span>
                    <span class="credential-value">{admin_email}</span>
                </div>
                <div class="credential-item">
                    <span class="credential-label">Temporary Password:</span>
                    <span class="credential-value">{password}</span>
                </div>
                <div class="credential-item">
                    <span class="credential-label">Login URL:</span>
                    <a href="{LOGIN_URL}">{LOGIN_URL}</a>
                </div>
            </div>

            <div class="warning">
                <strong>IMPORTANT:</strong> You must change your password on first login for security purposes.
            </div>

            <div class="steps">
                <h3>Getting Started</h3>
                <ol>
                    <li>Log in with your credentials above</li>
                    <li>Complete the onboarding wizard</li>
                    <li>Set up your organization profile</li>
                    <li>Invite team members</li>
                </ol>
            </div>

            <center>
                <a href="{LOGIN_URL}" class="button">Login Now</a>
            </center>

            <p>Need help? Contact <a href="mailto:support@shahin-ai.com">support@shahin-ai.com</a></p>

            <p>Best regards,<br><strong>Shahin AI Team</strong></p>
        </div>
        <div class="footer">
            <p>&copy; 2026 Shahin AI. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
"""

        # Attach both versions
        part1 = MIMEText(text_content, 'plain')
        part2 = MIMEText(html_content, 'html')
        msg.attach(part1)
        msg.attach(part2)

        # Send email
        with smtplib.SMTP(SMTP_CONFIG['host'], SMTP_CONFIG['port']) as server:
            server.starttls()
            server.login(SMTP_CONFIG['username'], SMTP_CONFIG['password'])
            server.sendmail(SMTP_CONFIG['from_email'], admin_email, msg.as_string())

        print(f"  [OK] Welcome email sent successfully to {admin_email}")
        return True

    except Exception as e:
        print(f"  [FAILED] Email sending failed: {e}")
        print(f"\n  Saving email content to file as fallback...")

        # Save email content to file as fallback
        try:
            filename = f"welcome_email_{tenant_name.replace(' ', '_')}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
            with open(filename, 'w', encoding='utf-8') as f:
                f.write(f"TO: {admin_email}\n")
                f.write(f"SUBJECT: Welcome to Shahin AI GRC Platform - Your Admin Credentials\n")
                f.write(f"FROM: {SMTP_CONFIG['from_email']}\n")
                f.write("-" * 60 + "\n\n")
                f.write(text_content)
            print(f"  [OK] Email content saved to: {filename}")
            print(f"\n  NOTE: Office 365 requires OAuth2 or App Password for SMTP.")
            print(f"  Please send the email manually or configure App Password.")
        except Exception as file_error:
            print(f"  [FAILED] Could not save email file: {file_error}")

        return False


def generate_aspnet_identity_password_hash(password: str) -> str:
    """
    Generate ASP.NET Core Identity V3 password hash.
    Format: Version (1 byte) + KDF (4 bytes) + Iterations (4 bytes) + Salt Size (4 bytes) + Salt + Derived Key
    """
    version = 1  # Version 3 uses 0x01
    kdf = 1  # HMAC-SHA256 (prf)
    iterations = 100000  # Default for V3
    salt_size = 16
    key_size = 32  # 256 bits

    # Generate random salt
    salt = secrets.token_bytes(salt_size)

    # Derive key using PBKDF2-HMAC-SHA256
    derived_key = hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, iterations, dklen=key_size)

    # Pack the header: version (1 byte) + kdf (4 bytes big-endian) + iterations (4 bytes big-endian) + salt size (4 bytes big-endian)
    header = struct.pack('>B', version)  # version
    header += struct.pack('>I', kdf)  # key derivation function
    header += struct.pack('>I', iterations)  # iterations
    header += struct.pack('>I', salt_size)  # salt size

    # Combine: header + salt + derived key
    password_hash = header + salt + derived_key

    # Return as base64
    return base64.b64encode(password_hash).decode('utf-8')


def create_tenant_and_admin():
    """Create the tenant and admin user in the database."""
    conn = None
    try:
        print(f"Connecting to database at {DB_CONFIG['host']}:{DB_CONFIG['port']}...")
        conn = psycopg2.connect(**DB_CONFIG)
        cur = conn.cursor()

        # Generate UUIDs
        tenant_id = str(uuid.uuid4())
        user_id = str(uuid.uuid4())
        tenant_user_id = str(uuid.uuid4())
        workspace_id = str(uuid.uuid4())
        correlation_id = str(uuid.uuid4())
        now = datetime.now(timezone.utc)

        # Check if tenant already exists
        cur.execute('SELECT "Id" FROM "Tenants" WHERE "TenantSlug" = %s', (TENANT_SLUG,))
        existing_tenant = cur.fetchone()
        if existing_tenant:
            print(f"Tenant '{TENANT_SLUG}' already exists. Using existing tenant.")
            tenant_id = existing_tenant[0]
        else:
            # Create tenant
            print(f"Creating tenant '{TENANT_NAME}'...")
            activation_token = base64.urlsafe_b64encode(secrets.token_bytes(32)).decode('utf-8')
            cur.execute('''
                INSERT INTO "Tenants" (
                    "Id", "TenantSlug", "OrganizationName", "AdminEmail", "Email",
                    "TenantCode", "BusinessCode", "Status", "IsActive",
                    "ActivationToken", "ActivatedAt", "ActivatedBy", "SubscriptionTier", "SubscriptionStartDate",
                    "CorrelationId", "DataIsolationLevel", "OnboardingStatus",
                    "CreatedDate", "CreatedBy", "IsDeleted"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s, %s,
                    %s, %s, %s,
                    %s, %s, %s
                )
            ''', (
                tenant_id, TENANT_SLUG, TENANT_NAME, ADMIN_EMAIL, ADMIN_EMAIL,
                TENANT_CODE, f"{TENANT_CODE}-TEN-2026-000001", 'Active', True,
                activation_token, now, 'system', 'Enterprise', now,
                correlation_id, 'Shared', 'NOT_STARTED',
                now, 'system', False
            ))
            print(f"  Tenant ID: {tenant_id}")

        # Check if user already exists in ApplicationUser (main user table for GRC)
        cur.execute('SELECT "Id" FROM "ApplicationUser" WHERE "Email" = %s', (ADMIN_EMAIL,))
        existing_user = cur.fetchone()
        if existing_user:
            print(f"User '{ADMIN_EMAIL}' already exists. Using existing user.")
            user_id = existing_user[0]
        else:
            # Generate password hash
            password_hash = generate_aspnet_identity_password_hash(ADMIN_PASSWORD)
            security_stamp = str(uuid.uuid4())
            concurrency_stamp = str(uuid.uuid4())

            # Create admin user in ApplicationUser (combined table with Identity + Profile fields)
            # This is referenced by TenantUsers
            print(f"Creating admin user '{ADMIN_EMAIL}' in ApplicationUser...")
            cur.execute('''
                INSERT INTO "ApplicationUser" (
                    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
                    "FirstName", "LastName", "Department", "JobTitle",
                    "IsActive", "CreatedDate", "MustChangePassword", "KsaCompetencyLevel"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s
                )
            ''', (
                user_id, ADMIN_EMAIL, ADMIN_EMAIL.upper(), ADMIN_EMAIL, ADMIN_EMAIL.upper(),
                True, password_hash, security_stamp, concurrency_stamp,
                False, False, True, 0,
                ADMIN_FIRST_NAME, ADMIN_LAST_NAME, 'Administration', 'Tenant Administrator',
                True, now, True, 0
            ))

            # Also create in AspNetUsers for role assignment (referenced by AspNetUserRoles)
            print(f"Creating admin user '{ADMIN_EMAIL}' in AspNetUsers...")
            cur.execute('''
                INSERT INTO "AspNetUsers" (
                    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s
                )
            ''', (
                user_id, ADMIN_EMAIL, ADMIN_EMAIL.upper(), ADMIN_EMAIL, ADMIN_EMAIL.upper(),
                True, password_hash, security_stamp, concurrency_stamp,
                False, False, True, 0
            ))
            print(f"  User ID: {user_id}")

        # Check if Admin role exists, create if not
        cur.execute('SELECT "Id" FROM "AspNetRoles" WHERE "Name" = %s', ('Admin',))
        role = cur.fetchone()
        if not role:
            role_id = str(uuid.uuid4())
            print("Creating 'Admin' role...")
            cur.execute('''
                INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
                VALUES (%s, %s, %s, %s)
            ''', (role_id, 'Admin', 'ADMIN', str(uuid.uuid4())))
        else:
            role_id = role[0]

        # Assign Admin role to user
        cur.execute('SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = %s AND "RoleId" = %s', (user_id, role_id))
        if not cur.fetchone():
            print("Assigning Admin role to user...")
            cur.execute('''
                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                VALUES (%s, %s)
            ''', (user_id, role_id))

        # Check if TenantUser link exists
        cur.execute('SELECT "Id" FROM "TenantUsers" WHERE "UserId" = %s AND "TenantId" = %s', (user_id, tenant_id))
        if not cur.fetchone():
            # Get or create role catalog entry
            cur.execute('SELECT "RoleCode" FROM "RoleCatalogs" WHERE "RoleCode" = %s AND "IsActive" = true', ('ADMIN',))
            role_catalog = cur.fetchone()
            role_code = 'ADMIN' if role_catalog else 'USER'

            # Get or create title catalog entry
            cur.execute('SELECT "TitleCode" FROM "TitleCatalogs" WHERE "TitleCode" = %s AND "IsActive" = true', ('ADMIN_TITLE',))
            title_catalog = cur.fetchone()
            title_code = 'ADMIN_TITLE' if title_catalog else 'USER_TITLE'

            # Create TenantUser link
            print("Linking user to tenant...")
            invitation_token = base64.urlsafe_b64encode(secrets.token_bytes(32)).decode('utf-8')
            cur.execute('''
                INSERT INTO "TenantUsers" (
                    "Id", "TenantId", "UserId", "RoleCode", "TitleCode",
                    "Status", "InvitationToken", "InvitedAt", "ActivatedAt", "InvitedBy",
                    "IsOwnerGenerated", "MustChangePasswordOnFirstLogin",
                    "CreatedDate", "IsDeleted"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s, %s,
                    %s, %s,
                    %s, %s
                )
            ''', (
                tenant_user_id, tenant_id, user_id, role_code, title_code,
                'Active', invitation_token, now, now, 'system',
                True, True,
                now, False
            ))

        # Update tenant with FirstAdminUserId
        cur.execute('''
            UPDATE "Tenants"
            SET "FirstAdminUserId" = %s, "AdminAccountGenerated" = true, "AdminAccountGeneratedAt" = %s
            WHERE "Id" = %s
        ''', (user_id, now, tenant_id))

        # Create default workspace if it doesn't exist
        cur.execute('SELECT "Id" FROM "Workspaces" WHERE "TenantId" = %s AND "IsDefault" = true', (tenant_id,))
        if not cur.fetchone():
            print("Creating default workspace...")
            workspace_code = f"WS-{TENANT_CODE}-001"
            cur.execute('''
                INSERT INTO "Workspaces" (
                    "Id", "TenantId", "WorkspaceCode", "Name", "WorkspaceType",
                    "DefaultLanguage", "Description", "IsDefault", "Status",
                    "CreatedDate", "CreatedBy", "IsDeleted"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s
                )
            ''', (
                workspace_id, tenant_id, workspace_code, f'{TENANT_NAME} Workspace', 'Standard',
                'en', f'Primary workspace for {TENANT_NAME}', True, 'Active',
                now, 'system', False
            ))

            # Update tenant with default workspace
            cur.execute('UPDATE "Tenants" SET "DefaultWorkspaceId" = %s WHERE "Id" = %s', (workspace_id, tenant_id))

        # Commit all changes
        conn.commit()

        print("\n" + "="*60)
        print("SUCCESS! Tenant and Admin created successfully!")
        print("="*60)
        print(f"\nTenant Details:")
        print(f"  Name: {TENANT_NAME}")
        print(f"  Slug: {TENANT_SLUG}")
        print(f"  ID: {tenant_id}")
        print(f"\nAdmin Credentials:")
        print(f"  Email: {ADMIN_EMAIL}")
        print(f"  Password: {ADMIN_PASSWORD}")
        print(f"  (Must change password on first login)")
        print("="*60)

        # Send welcome email
        send_welcome_email(TENANT_NAME, ADMIN_EMAIL, ADMIN_PASSWORD)

    except Exception as e:
        print(f"ERROR: {e}")
        if conn:
            conn.rollback()
        raise
    finally:
        if conn:
            conn.close()


if __name__ == '__main__':
    create_tenant_and_admin()
