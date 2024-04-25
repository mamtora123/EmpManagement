using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly MockEmployeeRepository _empRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _empRepo = new MockEmployeeRepository();
            _httpContextAccessor = httpContextAccessor;
        }

        //Action method for returning XML response
        public async Task<IActionResult> GetXmlSoap()
        {
            var users = await _empRepo.GetAllUserAsync();

            // For simplicity, let's manually build XML string here
            var xmlContent = "<UserList>";

            foreach (var user in users)
            {
                xmlContent += $"<UserList><Name>{user.Name}</Name><Email>{user.Email}</Email></UserList>";
            }

            xmlContent += "</UserList>";
            return Content(xmlContent, "application/xml");
        }


        // Action method to return JSON Response
        public async Task<IActionResult> GetJson()
        {
            var users = await _empRepo.GetAllUserAsync();
            return Json(users);
        }

        // Action method to demonstrate accessing HttpContext
        public IActionResult GetClientIPAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            return Content($"Client IP Address: {ipAddress}");
        }

        public IActionResult Index()
        {
            var model = _empRepo.GetAllEmployee();

            string removeMessage = TempData["RemoveMessage"] as string;
            string successMessage = TempData["SuccessMessage"] as string;

            // Pass the message to the view using ViewBag or ViewData
            ViewBag.SuccessMessage = successMessage;
            ViewData["RemoveMessage"] = removeMessage;

            model = GetRemovedList(model);

            model = GetUpdatedEmpList(model);           

            return View(model);
        }     

        public ViewResult Details(int Id)
        {

            var model = _empRepo.GetEmployee(Id);

            var _empVM = new EmployeeViewModel()
            {
                Id = model.Id,
                Name = model.Name,
                Email = model.Email,
                Department = model.Department
            };

            return View(_empVM);
        }

        //Partial View
        [HttpGet]
        public ActionResult EditPartial(int? Id)
        {
            var employee = _empRepo.GetEmployee(Id ?? 0);
            if (employee != null)
            {
                var editModel = new Employee
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    Department = employee.Department,
                };
                return PartialView("_EditPartial", editModel);
            }
            return PartialView("_EditPartial", null);
        }

        [HttpGet]
        public ActionResult Edit(int? Id)
        {
            //Model Binding
            if (Id == null)
            {
                return View(new Employee());
            }
            else
            {
                //get employee details
                var _empModel = _empRepo.GetEmployee(Id ?? 0);
                return View(_empModel);
            }
        }

        [HttpPost]
        public IActionResult Edit(Employee employeeModel)
        {
            if (ModelState.IsValid)
            {
                if (employeeModel.Id > 0)
                {
                    // If the employee ID is greater than 0, it means it's an existing employee
                    // Update the employee
                    var updatedEmployee = _empRepo.Update(employeeModel);

                    // Check if the update was successful
                    if (updatedEmployee != null)
                    {
                        // Serialize the updated employee object to JSON
                        string updatedEmployeeJson = JsonConvert.SerializeObject(updatedEmployee);
                        // Store the serialized JSON in TempData
                        TempData["UpdatedEmployee"] = updatedEmployeeJson;

                        TempData["SuccessMessage"] = MessageConstants.SuccessMessage;
                        // If the update was successful, redirect to the index action
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //View Data and View Bag
                        // If the update failed, return to the edit view with an error message
                        ViewBag.ErrorMessage = MessageConstants.FailedToUpdate;
                        return View(employeeModel);
                    }
                }
                else
                {
                    // If the employee ID is 0 or less, it means it's a new employee
                    // Add the employee to the database
                    var addedEmployee = _empRepo.Add(employeeModel);

                    // Check if the addition was successful
                    if (addedEmployee != null)
                    {
                        // If the addition was successful, redirect to the index action
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // If the addition failed, return to the edit view with an error message
                        ViewBag.ErrorMessage = MessageConstants.FailedToAdd;
                        return View(employeeModel);
                    }
                }
            }
            else
            {
                // If model validation fails, return to the edit view with validation errors
                return View(employeeModel);
            }
        }

        public IActionResult Remove(int Id)
        {
            //Remove Record
            if (Id > 0)
            {
                var removeEmployee = _empRepo.Remove(Id);
                if (removeEmployee != null)
                {
                    TempData["RemovedEmployeeId"] = removeEmployee.Id;
                }
                TempData["RemoveMessage"] = MessageConstants.RecordRemovedSuccessfully;
            }
            else
            {
                // Set a success message using ViewData
                TempData["RemoveMessage"] = MessageConstants.RecordNotFound;
            }
            //Redirect to index
            return RedirectToAction("Index");
        }

        private List<Employee> GetRemovedList(IEnumerable<Employee> employee)
        {
            var model = employee;
            if (ViewData["RemoveMessage"] != null)
            {
                // Get the removed employee ID from TempData
                var removedEmployeeId = TempData["RemovedEmployeeId"] as int?;

                //View Data and View Bag
                model = employee.Where(x => x.Id != removedEmployeeId);

            }
            return model.ToList();
        }

        private List<Employee> GetUpdatedEmpList(IEnumerable<Employee> employee)
        {
            // Get the updated employee from TempData and deserialize it from JSON
            var updatedEmployeeJson = TempData["UpdatedEmployee"] as string;
            var updatedEmployee = !string.IsNullOrEmpty(updatedEmployeeJson) ? JsonConvert.DeserializeObject<Employee>(updatedEmployeeJson) : null;

            // If an employee was updated, find and update it in the model
            if (updatedEmployee != null)
            {
                for (int i = 0; i < employee.ToList().Count; i++)
                {
                    if (employee.ToList()[i].Id == updatedEmployee.Id)
                    {
                        employee.ToList()[i].Name = updatedEmployee.Name; 
                        employee.ToList()[i].Email = updatedEmployee.Email; 
                        employee.ToList()[i].Department = updatedEmployee.Department; 
                        break; // Exit the loop once the employee is found and updated
                    }
                }
            }
            return employee.ToList();
        }
    }
}
