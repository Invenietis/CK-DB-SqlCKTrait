-- SetupConfig: { "Requires": "CK.sCKTraitContextSeparatorSet" }
--
--	Registers a context identified by its name with its separator.
--  For an existing context, the provided separator is updated if it has changed.
--  An error is raised if any atomic trait used by the context contains the new seprator:
--  see CK.sCKTraitContextSeparatorSet. 
--
alter procedure CK.sCKTraitContextRegister
(
	@ActorId int,
	@ContextName varchar(128),
	@Separator char,
	@CKTraitContextIdResult int output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    set @ContextName = ltrim(rtrim(@ContextName));
    if @ContextName is null or len(@ContextName) = 0 throw 50000, 'Argument.MustNotBeNullOrEmpty', 1;
    if @Separator is null throw 50000, 'Argument.MustNotBeNull', 1;

	--[beginsp]

	declare @CurSeparator char;
	select @CKTraitContextIdResult = CKTraitContextId, @CurSeparator = Separator
		from CK.tCKTraitContext
		where ContextName = @ContextName;

	if @CurSeparator is null
	begin
		--<PreCreate /> 

		insert into CK.tCKTraitContext( ContextName, Separator ) values( @ContextName, @Separator );
  		set @CKTraitContextIdResult = scope_identity();

		--<PostCreate revert /> 
	end
	else if @CurSeparator <> @Separator
	begin
		exec CK.sCKTraitContextSeparatorSet @ActorId, @CKTraitContextIdResult, @Separator;
	end

	--[endsp]
end
