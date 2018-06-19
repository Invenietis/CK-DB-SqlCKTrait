-- SetupConfig: {}
--
--	This updates the context separator.
--  An error is raised if any atomic trait used by the context contains the new seprator.
--  On success, the composite TraitNames are updated. 
--
alter procedure CK.sCKTraitContextSeparatorSet
(
	@ActorId int,
	@CKTraitContextId int,
	@NewSeparator char
)
as
begin
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @NewSeparator is null throw 50000, 'Argument.MustNotBeNull', 1;
	if @CKTraitContextId is null or @CKTraitContextId <= 0 throw 50000, 'Argument.InvalidCKTraitContextId', 1;

	--[beginsp]

	declare @OldSeparator char;
	select @OldSeparator = Separator from CK.tCKTraitContext where CKTraitContextId = @CKTraitContextId and Separator != @NewSeparator;
	if @OldSeparator is not null
	begin
		if exists( select 1
                   from CK.tCKTrait t 
					where t.CKTraitContextId = CKTraitContextId and charindex( @NewSeparator, t.TraitName ) > 0 )
		begin
			;throw 50000, 'CKTrait.NewSeparatorConflicts', 1;
		end

		--<PreSet /> 

		update CK.tCKTrait set TraitName = replace(TraitName,@OldSeparator,@NewSeparator) where CKTraitContextId = @CKTraitContextId;
		update CK.tCKTraitContext set Separator = @NewSeparator where CKTraitContextId = @CKTraitContextId;

		--<PostSet revert /> 
	end

	--[endsp]
end
