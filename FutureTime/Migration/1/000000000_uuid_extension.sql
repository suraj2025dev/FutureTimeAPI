create extension if not exists "uuid-ossp";

CREATE OR REPLACE FUNCTION public.fn_new_uuid() RETURNS uuid AS $$
BEGIN
    RETURN uuid_generate_v4();
END;
$$ LANGUAGE plpgsql;
