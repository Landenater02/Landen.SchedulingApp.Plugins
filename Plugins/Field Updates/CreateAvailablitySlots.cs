using Landen.SchedulingApp.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Web.UI.WebControls;

namespace plugins
{
    /// <summary>
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class CreateAvailabilitySlots : PluginBase
    {

        private static readonly Dictionary<string, int> DayToChoiceValue = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "Sunday", 854020000 },
        { "Monday", 854020001 },
        { "Tuesday", 854020002 },
        { "Wednesday", 854020003 },
        { "Thursday", 854020004 },
        { "Friday", 854020005 },
        { "Saturday", 854020006 }
    };
        public CreateAvailabilitySlots(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(CreateAvailabilitySlots))
        {
            // TODO: Implement your custom configuration handling
            // https://docs.microsoft.com/powerapps/developer/common-data-service/register-plug-in#set-configuration-data
        }

        // Entry point for custom business logic execution
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;

            // TODO: Implement your custom business logic

            // Check for the entity on which the plugin would be registered
            if (!(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity))
            {
                throw new InvalidPluginExecutionException($"Invalid Target");
            }

            var EmployeeAvailability = (Entity)context.InputParameters["Target"];

            if (!EmployeeAvailability.LogicalName.Equals("ljh_employeeavailability"))
                throw new InvalidPluginExecutionException($"Invalid Entity, expected ljh_employeeavailability but got " + EmployeeAvailability.LogicalName);
                

            if(!EmployeeAvailability.Contains("ljh_employee") || EmployeeAvailability["ljh_employee"] == null)
                throw new InvalidPluginExecutionException($"Invalid Employee Field");


            localPluginContext.Trace("Tracing: Grabbing Employee");//<<<


            EntityReference Employee = EmployeeAvailability.GetAttributeValue<EntityReference>("ljh_employee");


            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);


            QueryExpression query = new QueryExpression("ljh_daysofweek")
            {
                ColumnSet = new ColumnSet(true) // true = retrieve all columns
            };


            localPluginContext.Trace("Tracing: Days");//<<<
            EntityCollection days = service.RetrieveMultiple(query);


            foreach (Entity day in days.Entities)
            {

                string dayName = day.GetAttributeValue<string>("ljh_day");
                Guid dayId = day.Id;

                localPluginContext.Trace("Tracing: Creating Slot For " + dayName);//<<<

                if (string.IsNullOrWhiteSpace(dayName) || !DayToChoiceValue.ContainsKey(dayName))
                    continue;

                // Time construction
                DateTime startTime = DateTime.Today.AddHours(5); // 5:00 AM
                DateTime endTime = dayName.Equals("Sunday", StringComparison.OrdinalIgnoreCase)
                    ? DateTime.Today.AddHours(14.5) // 2:30 PM
                    : DateTime.Today.AddHours(20);  // 8:00 PM

                // Create availability slot
                Entity slot = new Entity("ljh_availabilityslot");
                slot["ljh_dayofweek"] = new EntityReference("ljh_daysofweek", dayId);
                slot["ljh_dayofweekchoice"] = new OptionSetValue(DayToChoiceValue[dayName]);
                slot["ljh_employeeavailability"] = new EntityReference("ljh_employeeavailability", EmployeeAvailability.Id);
                slot["ljh_employee"] = Employee;
                slot["ljh_start"] = startTime;
                slot["ljh_startview"] = "5:00 AM";
                slot["ljh_end"] = endTime;
                slot["ljh_endview"] = dayName.Equals("Sunday", StringComparison.OrdinalIgnoreCase) ? "2:30 PM" : "8:00 PM";

                service.Create(slot);
            }

            localPluginContext.Trace("Tracing: Successfully Created Associated Availability Slot");//<<<
        }
    }
}
