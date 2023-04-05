namespace Base.Domain;

public class LangStr : Dictionary<string, string>
{

    private const string DefaultCulture = "en";

    private string GetCultureName(string culture)
    {
        return culture.Split("-")[0];
    }

    public LangStr()
    {
    }

    public LangStr(string value) : this(value, Thread.CurrentThread.CurrentUICulture.Name)
    {
    }

    public LangStr(string value, string culture)
    {
        this[GetCultureName(culture)] = value;
    }

    public string? Translate(string? culture = null)
    {
        if (Count == 0) return null;

        culture = culture?.Trim() ?? Thread.CurrentThread.CurrentUICulture.Name;
        culture = GetCultureName(culture);
        
        if (ContainsKey(culture))
        {
            return this[culture];
        }
        
        var neutralCulture = culture.Split("-")[0];
        if (ContainsKey(neutralCulture))
        {
            return this[neutralCulture];
        }
        
        if (ContainsKey(DefaultCulture))
        {
            return this[DefaultCulture];
        }

        return null;
    }

    public void SetTranslation(string value)
    {
        this[GetCultureName(Thread.CurrentThread.CurrentUICulture.Name)] = value;
    }

    public override string ToString()
    {
        return Translate() ?? "???";
    }

    // string xxx = new LangStr("zzz");
    public static implicit operator string(LangStr? langStr) => langStr?.ToString() ?? "null";
    
    // LangStr lStr = "xxx"; // internally it will be lStr = new LangStr("xxx");
    public static implicit operator LangStr(string value) => new(value);
}