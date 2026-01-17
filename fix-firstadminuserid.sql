-- Add FirstAdminUserId column to Tenants table if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Tenants' 
        AND column_name = 'FirstAdminUserId'
    ) THEN
        ALTER TABLE "Tenants" ADD COLUMN "FirstAdminUserId" text;
    END IF;
END $$;
