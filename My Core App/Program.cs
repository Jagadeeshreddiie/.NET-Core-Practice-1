using System.Text.Json;
using static EmployeeRepo;

var builder = WebApplication.CreateBuilder(args);  // Sets Up the Kestrel Server
var app = builder.Build();                          // Return the Web Application instance

//app.MapGet("/", () => "value Hiii" );     // Middleware Component (one of)      -- Minimal API Type


app.Run( async (HttpContext context) =>
{
    var path=context.Request.Path;  
    if (context.Request.Method == "GET") {
        if (path == "/")
        {
            await context.Response.WriteAsync(context.Request.Method+"\n");
            await context.Response.WriteAsync(context.Request.Path + "\n");
            await context.Response.WriteAsync(context.Request.Protocol + "\n");
        }
        else if (path == "/employees")
        {
            foreach(var emp in EmployeeRepo.GetEmployees())
            {
                await context.Response.WriteAsync($"ID: {emp.ID}, Name: {emp.Name}, Position: {emp.Position}, Salary: {emp.Salary}\n");
            }
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
        }
    }
    else if (context.Request.Method == "POST")
    {
        if (path == "/employess")
        {
            using var body = new StreamReader(context.Request.Body);
            var employee = JsonSerializer.Deserialize<Employee>(await body.ReadToEndAsync());
            EmployeeRepo.AddEmployee(employee);
        }
    }
});

app.Run();    // runs the web application on infinite loop to receive the HTTP requests 
              // Converts the HTTP Request to HTTP Context Object.


// Dummy Class of Employees for the GET & POST


public static class EmployeeRepo
{
    public static List<Employee> Employees = new List<Employee>
    {
        new Employee(1,"John Doe","Software Engineer",60000),
        new Employee(2,"Jane Smith","Project Manager",75000),
        new Employee(3,"Mike Johnson","QA Analyst",50000)
    };

    public static List<Employee> GetEmployees() => Employees.ToList();

    public static void AddEmployee(Employee? employe)
    {
        if (employe != null)
        {
            Employees.Add(employe);
        }
    }
}
public class Employee
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }

    public Employee(int ID, string Name, string Position, decimal Salary)
    {
        this.ID = ID;
        this.Name = Name;
        this.Position = Position;
        this.Salary = Salary;
    }
}