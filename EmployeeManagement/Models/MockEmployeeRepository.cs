using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository 
    {
        private List<Employee> _empList;
        private readonly List<UserModel> _userList;

        public MockEmployeeRepository()
        {
            _empList = new List<Employee>()

            {
                new Employee(){Id=1, Name="Mark", Department =Dept.IT, Email="Mark@hotmail.com"},
                new Employee(){Id=2, Name="Param", Department=Dept.IT, Email="param@hotmail.com"},
                new Employee(){Id=3, Name="Tom", Department= Dept.HR, Email="Tom@hotmail.com"}
            };

            _userList = new List<UserModel>()

            {
                new UserModel(){Name="John", Email="john@gmail.com"},
                new UserModel(){Name="Vicky", Email="vicky@hotmil.com"},
                new UserModel(){Name="Mike", Email="mike@hotmil.com"},
                new UserModel(){Name="Peter", Email="peter@yahoo.com"},
                new UserModel(){Name="sam", Email="sam@yahoo.com"},
            };
        }

        public async Task<List<UserModel>> GetAllUserAsync()
        {
            // Simulating asynchronous behavior by wrapping the list in a Task
            await Task.Delay(100); // Simulate delay (optional)
            return _userList;
        }


        public IEnumerable<Employee> GetAllEmployee()
        {
            return _empList;
        }

        public Employee GetEmployee(int Id)
        {
            return _empList.FirstOrDefault(emp => emp.Id == Id);
        }
        public Employee Add(Employee employee)
        {
            employee.Id = _empList.Max(e => e.Id) + 1;
            _empList.Add(employee);
            return employee;
        }

        public Employee Update(Employee employeeUpdate)
        {
            var emp = _empList.FirstOrDefault(e => e.Id == employeeUpdate.Id);
            if (emp != null)
            {
                emp.Name = employeeUpdate.Name;
                emp.Email = employeeUpdate.Email;
                emp.Department = employeeUpdate.Department;          
            }            
            return emp;
        }
        public Employee Remove(int Id)
        {
            var empToRemove = _empList.FirstOrDefault(emp => emp.Id == Id);
            Employee removedEmployee = null;

            if (empToRemove != null)
            {
                // Create a copy of the list
                var tempList = new List<Employee>(_empList);

                // Remove the employee from the copy
                tempList.Remove(empToRemove);

                // Assign the removed employee to the original list
                removedEmployee = empToRemove;

                // Update the original list with the modified copy
                _empList = tempList;
            }

            return removedEmployee;
        }

    }
}
