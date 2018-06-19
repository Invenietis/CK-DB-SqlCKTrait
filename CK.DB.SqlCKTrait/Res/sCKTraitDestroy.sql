-- SetupConfig: {}
--
--   Destroys an existing trait (does nothing if the trait does not exist). 
--   If the trait is atomic and its value is currently used in a composite 
--   trait of the same context an error is raised.
-- 
--   To remove an atomic trait, all composite traits that use it must 
--   already be destroyed. This is because traits are "values". 
--   Removing the atomic value would create different traits that 
--   share the same value.
--
alter procedure CK.sCKTraitDestroy
(
	@ActorId int,
	@CKTraitId int
)
as
begin
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @CKTraitId is null or @CKTraitId <= 0 throw 50000, 'Argument.InvalidCKTraitId', 1;

	--[beginsp]

	declare @CKContextTraitId int;
	declare @AtomicWordId int;
	select @CKContextTraitId = CKTraitContextId, @AtomicWordId = AtomicWordId
		from CK.tCKTrait where CKTraitId = @CKTraitId;
	if @AtomicWordId is not null
	begin
        -- Before removing an atomic trait we must check 
        -- that it does not belong to another CKTrait: to remove an atomic
        -- trait, all traits that use it must already be destroyed. This is
        -- because traits are "values". Removing the word would create
        -- different traits that share the same value.
		if @AtomicWordId <> 0
		begin
			--<PreAtomicDestroy revert /> 

			if exists( select 1 from CK.tCKTraitSet tS
						inner join CK.tCKTrait t on t.CKTraitId = tS.CKTraitId
						where tS.CKTraitWordId = @AtomicWordId and t.CKTraitContextId = @CKContextTraitId and t.CKTraitId <> @CKTraitId )
			begin
				;throw 50000, 'CKTrait.CKTraitAtomicInUse', 1;
			end
	  
			delete CK.tCKTraitSet where CKTraitId = @CKTraitId;
			delete CK.tCKTrait where CKTraitId = @CKTraitId;

			--<PostAtomicDestroy />
		end
		else
		begin
			--<PreDestroy revert /> 

			delete CK.tCKTraitSet where CKTraitId = @CKTraitId;
			delete from CK.tCKTrait where CKTraitId = @CKTraitId;
      
		    --<PostDestroy />
	   end
	end
    --[endsp]
end

