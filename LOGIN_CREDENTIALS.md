# GRC System Login Credentials

## Admin Users Created

### 1. Primary Admin User
- **Email**: `admin@grc.com`
- **Password**: `Admin123!`
- **Role**: Admin
- **Status**: ✅ Created and active

### 2. Support Admin User
- **Email**: `support@shahin-ai.com`
- **Password**: `DogCon@Admin`
- **Role**: Admin
- **Status**: ✅ Created and active

## Access URLs

### MVC Application
- **URL**: http://localhost:8888
- **Login Page**: http://localhost:8888/Account/Login

### Blazor Application
- **URL**: http://localhost:8082
- **API**: http://localhost:5010

## Database Users

Both users have been created in the PostgreSQL database with:
- Email confirmed: Yes
- Account locked: No
- Two-factor auth: Disabled
- Admin role: Assigned

## How to Login

1. Navigate to http://localhost:8888/Account/Login
2. Enter one of the credentials above
3. Click the Login button

## Troubleshooting

If login fails with "Invalid login attempt":
1. The password hash may need to be regenerated
2. Check container logs: `docker logs grc-system-grcmvc-1`
3. Verify database connection: Container may have DB connection issues

## Notes

- The support@shahin-ai.com account was specifically requested by the user
- Both accounts have full admin privileges
- Passwords are case-sensitive
- The system uses ASP.NET Core Identity for authentication