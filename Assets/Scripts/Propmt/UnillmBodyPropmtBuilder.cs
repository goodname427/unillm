namespace unillm
{
    public class UnillmBodyPropmtBuilder
    {
        public IUnillmBody Body { get; set; }

        public UnillmBodyPropmtBuilder(IUnillmBody body = null)
        {
            Body = body;
        }

        public string Build()
        {
            var argsPropmt = new UnillmTypePropmtBuilder(Body.ArgsType).Build();

            return $@"
{Body.Name}:
{Body.Description}
{argsPropmt}
";
        }
    }
}
