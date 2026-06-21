namespace Domain.Constants;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Instructor = "Instructor";
    public const string Student = "Student";

    public static readonly string[] All = [Admin, Instructor, Student];
}
