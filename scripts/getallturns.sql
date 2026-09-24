-- FUNCTION: public.getallturns()

DROP FUNCTION IF EXISTS public.getallturns();

CREATE OR REPLACE FUNCTION public.getallturns(
	)
    RETURNS TABLE(id uuid, name text, dni bigint, medicid uuid, medicname text, "Date" text, "Time" text, timeid uuid, socialwork text, reason text, accessed boolean, ismedic boolean) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
BEGIN
    RETURN QUERY
	SELECT t."Id", t."Name", t."Dni", t."MedicId", m."Name" as MedicName, TO_CHAR(t."DateTurn", 'DD/MM/YYYY'), 
			tt."Time", tt."Id" AS TimeId, t."SocialWork", t."Reason", t."Accessed", false
		FROM "Turns" t 
		INNER JOIN "Medics" m ON t."MedicId" = m."Id" 
		INNER JOIN "TimeTurns" tt ON t."TimeId" = tt."Id"
		WHERE t."DateTurn" > CURRENT_DATE - INTERVAL '3 months';
END;
$BODY$;

ALTER FUNCTION public.getallturns()
    OWNER TO postgres;

