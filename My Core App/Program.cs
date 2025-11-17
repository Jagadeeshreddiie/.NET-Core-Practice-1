using System.Text.Json;
using static EmployeeRepo;

var builder = WebApplication.CreateBuilder(args);  // Sets Up the Kestrel Server
var app = builder.Build();                          // Return the Web Application instance

//app.MapGet("/", () => "value Hiii" );     // Middleware Component (one of)      -- Minimal API Type


app.Run( async (HttpContext context) =>
{
    var path=context.Request.Path;
    var method = context.Request.Method;

    if (method == "GET")
    {
        if (path == "/")
        {
            await context.Response.WriteAsync(context.Request.Method + "\n");
            await context.Response.WriteAsync(context.Request.Path + "\n");
            await context.Response.WriteAsync(context.Request.Protocol + "\n");
        }
        else if (path.StartsWithSegments("/employees"))
        {
            foreach (var emp in EmployeeRepo.GetEmployees())
            {
                await context.Response.WriteAsync($"\nID: {emp.ID}, Name: {emp.Name}, Position: {emp.Position}, Salary: {emp.Salary}\n");
            }
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
        }
    }
    else if (method == "POST")
    {
        if (path.StartsWithSegments("/employess"))
        {
            using var body = new StreamReader(context.Request.Body);
            var employee = JsonSerializer.Deserialize<Employee>(await body.ReadToEndAsync());
            EmployeeRepo.AddEmployee(employee);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
        }
    }

    else if (method == "PUT")
    {
        if (path.StartsWithSegments("/employess"))
        {
            using var body = new StreamReader(context.Request.Body);
            var updatedEmployee = JsonSerializer.Deserialize<Employee>(await body.ReadToEndAsync());
            bool status = EmployeeRepo.UpdateEmployee(updatedEmployee);
            if (status)
                await context.Response.WriteAsync("\nEmployee Updated Successfully.");
            else
                await context.Response.WriteAsync("\nEmployee not found.");
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
        }
    }

    //Commented the Above code for the Query String Practice.


    //-- Params - Query String Practice--


   //await context.Response.WriteAsync(context.Request.QueryString.ToString());
   // foreach (var item in context.Request.Query.Keys)
   //     {
   //         await context.Response.WriteAsync($"\n{item} : {context.Request.Query[item]}");
   //     }


    // DELETE Req Practice

    else if (context.Request.Method == "DELETE")
    {
        if (context.Request.Path.StartsWithSegments("/Employee/Delete"))
        {
            if (context.Request.Query.ContainsKey("id"))
            {
                var id =context.Request.Query["id"];
                if(decimal.TryParse(id,out decimal employeeID))
                {

                    // Headers concept.
                    // We should not send all the data in the parameter or query string for security reasons.
                    // So , we use Headers to send sensitive data like Authorization tokens, API keys, etc.
                    // Here, we are using a simple Authorization header for demonstration purposes.

                    if (context.Request.Headers["Authorization"] == "admin")
                    {
                        bool status = EmployeeRepo.DeleteEmployee(employeeID);
                        if (status)
                        {
                            await context.Response.WriteAsync("Employee Deleted Successfully");
                        }
                        else
                        {
                            await context.Response.WriteAsync("Employee not found.");
                        }
                    }
                    else
                    {
                        context.Response.StatusCode=401;
                        await context.Response.WriteAsync("You are not Authorized to delete.");
                    }
                }
            }
            else
            {
                context.Response.StatusCode=404;
                await context.Response.WriteAsync("Invalid Request");
            }
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

    public static bool UpdateEmployee(Employee ? employee)
    {
        if(employee != null)
        {
            var Details = Employees.FirstOrDefault(a => a.ID == employee.ID);
            if (Details != null)
            {
                Details.Name= employee.Name;
                Details.Position= employee.Position;
                Details.Salary= employee.Salary;

                return true;
            }
        }
        return false;
    }

    public static bool DeleteEmployee(decimal id)
    {
        var employee = Employees.FirstOrDefault(e => e.ID == id);
        if (employee != null)
        {
            Employees.Remove(employee);
            return true;
        }
        return false;
    }
}
public class Employee
{
    public decimal ID { get; set; }
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
