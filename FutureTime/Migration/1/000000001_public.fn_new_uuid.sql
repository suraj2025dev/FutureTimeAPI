--It is used in all tables for default valud of UID column
-- FUNCTION: public.new_uuid()

-- DROP FUNCTION public.new_uuid();


 
CREATE OR REPLACE FUNCTION public.fn_new_uuid(
	)
    RETURNS uuid
    LANGUAGE 'sql'

    COST 100
    VOLATILE 
    
AS $BODY$
  --SELECT uuid_in(overlay(overlay(md5(random()::text || ':' || clock_timestamp()::text) placing '4' from 13) placing to_hex(floor(random()*(11-8+1) + 8)::int)::text from 17)::cstring);
  SELECT uuid_generate_v4();
$BODY$;

ALTER FUNCTION public.fn_new_uuid()
    OWNER TO postgres;
