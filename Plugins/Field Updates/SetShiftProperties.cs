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
    public class SetShiftProperties : PluginBase
    {
        public SetShiftProperties(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(SetShiftProperties))
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
            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
            // TODO: Implement your custom business logic

            // Check for the entity on which the plugin would be registered
            if (!(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity))
            {
                throw new InvalidPluginExecutionException($"Invalid Target");
            }

            var Shift = (Entity)context.InputParameters["Target"];

            if (!Shift.LogicalName.Equals("ljh_shift"))
                throw new InvalidPluginExecutionException($"Invalid Entity, expected ljh_shift but got " + Shift.LogicalName);


            // Validate required fields exist
            if (!Shift.Contains("ljh_shiftdate") || Shift["ljh_shiftdate"] == null)
                throw new InvalidPluginExecutionException("Shift date is missing.");

            if (!Shift.Contains("ljh_starttime") || Shift["ljh_starttime"] == null)
                throw new InvalidPluginExecutionException("Start time is missing.");

            if (!Shift.Contains("ljh_endtime") || Shift["ljh_endtime"] == null)
                throw new InvalidPluginExecutionException("End time is missing.");


            if (Shift.Contains("ljh_availabilityslot") && Shift["ljh_availabilityslot"] != null)
            {
                Entity availabilitySlot = service.Retrieve("ljh_availabilityslot",Shift.GetAttributeValue<EntityReference>("ljh_availabilityslot").Id,new ColumnSet("ljh_employee"));
                EntityReference employeeRef = availabilitySlot.GetAttributeValue<EntityReference>("ljh_employee");
                Shift["ljh_employee"] = employeeRef;
            }
            if(!Shift.Contains("ljh_employee") || Shift["ljh_employee"] == null)
                throw new InvalidPluginExecutionException("Employee is missing.");


            DateTime shiftDate = Shift.GetAttributeValue<DateTime>("ljh_shiftdate").Date;
            DateTime originalStart = Shift.GetAttributeValue<DateTime>("ljh_starttime");
            DateTime originalEnd = Shift.GetAttributeValue<DateTime>("ljh_endtime");

            // Combine shift date with original time
            DateTime newStart = shiftDate.Add(originalStart.TimeOfDay);
            DateTime newEnd = shiftDate.Add(originalEnd.TimeOfDay);

            // Format times and date
            string datePart = shiftDate.ToString("M/d"); 
            string startPart = newStart.ToUniversalTime().ToString("h:mmtt");
            string endPart = newEnd.ToUniversalTime().ToString("h:mmtt");


            // Get employee name
            Entity employee = service.Retrieve(
                "ljh_employee",
                Shift.GetAttributeValue<EntityReference>("ljh_employee").Id,
                new ColumnSet("ljh_employeename")
            );

            string employeeName = employee.GetAttributeValue<string>("ljh_employeename") ?? "(No Name)";

            // Build shift name string
            string shiftName = $"{datePart} {employeeName} {startPart} - {endPart}";

            // Set final values
            Shift["ljh_starttime"] = newStart;
            Shift["ljh_endtime"] = newEnd;
            Shift["ljh_shift1"] = shiftName;


        }
    }
}
