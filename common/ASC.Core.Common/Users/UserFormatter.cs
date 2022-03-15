namespace ASC.Core.Users;

[Singletone]
public class UserFormatter : IComparer<UserInfo>
{
    private readonly DisplayUserNameFormat _format;
    private static bool _forceFormatChecked;
    private static string _forceFormat;

    public UserFormatter(IConfiguration configuration)
    {
        _format = DisplayUserNameFormat.Default;
        _configuration = configuration;
        UserNameRegex = new Regex(_configuration["core:username:regex"] ?? "");
    }

    public string GetUserName(UserInfo userInfo, DisplayUserNameFormat format)
    {
        ArgumentNullException.ThrowIfNull(userInfo);

        return string.Format(GetUserDisplayFormat(format), userInfo.FirstName, userInfo.LastName);
    }

    public string GetUserName(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            throw new ArgumentException();
        }

        return string.Format(GetUserDisplayFormat(DisplayUserNameFormat.Default), firstName, lastName);
    }

    public string GetUserName(UserInfo userInfo)
    {
        return GetUserName(userInfo, DisplayUserNameFormat.Default);
    }

    int IComparer<UserInfo>.Compare(UserInfo x, UserInfo y)
    {
        return Compare(x, y, _format);
    }

    public static int Compare(UserInfo x, UserInfo y)
    {
        return Compare(x, y, DisplayUserNameFormat.Default);
    }

    public static int Compare(UserInfo x, UserInfo y, DisplayUserNameFormat format)
    {
        if (x == null && y == null)
        {
            return 0;
        }
        if (x == null && y != null)
        {
            return -1;
        }
        if (x != null && y == null)
        {
            return +1;
        }

        if (format == DisplayUserNameFormat.Default)
        {
            format = GetUserDisplayDefaultOrder();
        }

        int result;
        if (format == DisplayUserNameFormat.FirstLast)
        {
            result = string.Compare(x.FirstName, y.FirstName, true);
            if (result == 0)
            {
                result = string.Compare(x.LastName, y.LastName, true);
            }
        }
        else
        {
            result = string.Compare(x.LastName, y.LastName, true);
            if (result == 0)
            {
                result = string.Compare(x.FirstName, y.FirstName, true);
            }
        }

        return result;
    }

    private static readonly Dictionary<string, Dictionary<DisplayUserNameFormat, string>> _displayFormats = new Dictionary<string, Dictionary<DisplayUserNameFormat, string>>
        {
            { "ru", new Dictionary<DisplayUserNameFormat, string>{ { DisplayUserNameFormat.Default, "{1} {0}" }, { DisplayUserNameFormat.FirstLast, "{0} {1}" }, { DisplayUserNameFormat.LastFirst, "{1} {0}" } } },
            { "default", new Dictionary<DisplayUserNameFormat, string>{ {DisplayUserNameFormat.Default, "{0} {1}" }, { DisplayUserNameFormat.FirstLast, "{0} {1}" }, { DisplayUserNameFormat.LastFirst, "{1}, {0}" } } },
        };


    private string GetUserDisplayFormat(DisplayUserNameFormat format)
    {
        if (!_forceFormatChecked)
        {
            _forceFormat = _configuration["core:user-display-format"];
            if (string.IsNullOrEmpty(_forceFormat))
            {
                _forceFormat = null;
            }

            _forceFormatChecked = true;
        }
        if (_forceFormat != null)
        {
            return _forceFormat;
        }

        var culture = Thread.CurrentThread.CurrentCulture.Name;
        if (!_displayFormats.TryGetValue(culture, out var formats))
        {
            var twoletter = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (!_displayFormats.TryGetValue(twoletter, out formats))
            {
                formats = _displayFormats["default"];
            }
        }

        return formats[format];
    }

    public static DisplayUserNameFormat GetUserDisplayDefaultOrder()
    {
        var culture = Thread.CurrentThread.CurrentCulture.Name;
        if (!_displayFormats.TryGetValue(culture, out var formats))
        {
            var twoletter = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (!_displayFormats.TryGetValue(twoletter, out formats))
            {
                formats = _displayFormats["default"];
            }
        }
        var format = formats[DisplayUserNameFormat.Default];

        return format.IndexOf("{0}") < format.IndexOf("{1}") ? DisplayUserNameFormat.FirstLast : DisplayUserNameFormat.LastFirst;
    }

    public Regex UserNameRegex { get; set; }

    private readonly IConfiguration _configuration;

    public bool IsValidUserName(string firstName, string lastName)
    {
        return UserNameRegex.IsMatch(firstName + lastName);
    }
}
