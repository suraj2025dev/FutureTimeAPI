DROP table if exists pms.tbl_user;
DROP table if exists pms.tbl_user_log;

create table pms.tbl_user
(
    id serial,
    uid uuid DEFAULT public.fn_new_uuid() not null,
    full_name character varying(100) COLLATE pg_catalog."default",
    forget_password_otp character varying(6) COLLATE pg_catalog."default",
    forget_password_otp_valid_till timestamp without time zone NULL,
    email character varying(500) COLLATE pg_catalog."default",
    password character varying(100) COLLATE pg_catalog."default",
    is_verified boolean NOT NULL DEFAULT true,
    is_locked boolean NOT NULL DEFAULT false,
    is_blocked boolean NOT NULL DEFAULT false,
	
    is_active boolean NOT NULL DEFAULT true,
    is_deleted boolean NOT NULL DEFAULT false,
    created_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    created_date timestamp without time zone NOT NULL DEFAULT now(),
    generated_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    generated_on timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT tbl_user_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE pms.tbl_user
    OWNER to postgres;

     ----CREATING LOG TABLE---
create table  pms.tbl_user_log as
select * from pms.tbl_user