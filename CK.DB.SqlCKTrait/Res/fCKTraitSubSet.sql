-- SetupConfig: {}
--   
--   Returns a one-column table with all the CKTraitId that are sub sets
--   of the provided trait identifier within the same context.
--
alter function [CK].[fCKTraitSubSet]( @CKTraitId int ) 
returns table
    return select t2.CKTraitId
            from CK.tCKTrait t 
            inner join CK.tCKTrait t2 on t2.CKTraitContextId = t.CKTraitContextId
            where t.CKTraitId = @CKTraitId 
            and not exists
                ( 
                    ( select CKTraitWordId 
                        from CK.tCKTraitSet tS
                        where CKTraitId = t2.CKTraitId
                    ) 
                    except
                    (select CKTraitWordId from CK.tCKTraitSet where CKTraitId = @CKTraitId)
                );


