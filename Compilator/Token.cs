namespace RedRust
{
    internal interface Token
    {
        void Compile(string tabs, StreamWriter sw);
    }
}
