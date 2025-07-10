using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landen.SchedulingApp.Plugins
{
    /// <summary>
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class generateSchedule : PluginBase
    {

        // <Positions, queue<employees>
        Dictionary<Entity, Queue<Entity>> shiftsMap = new Dictionary<Entity, Queue<Entity>>();
        Dictionary<Entity,Dictionary<String,int>> positionCounts = new Dictionary<Entity,Dictionary<String, int>>();
        //<days<position,count>>, put in the day, then put in the position, get the count

        public generateSchedule(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(generateSchedule))
        {
            // TODO: Implement your custom configuration handling
            // https://docs.microsoft.com/powerapps/developer/common-data-service/register-plug-in#set-configuration-data
        }

        /*
         * 
         * goal here is to Generate the schedule automatically. Bound to ljh_autogeneratescheduledata entity
         * some sort of action will create a new record for the this entity, which will trigger this plugin
         * current plan for fields will be start date, but more could be added depending on customization needs for the automatic schedule generation
         * 
         * this process can run asynchronously, we don't need to worry about effeciency here too much, as it will be run in the background
         */

        // Entry point for custom business logic execution
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);

            /***
             * 
             * Metrics to go by:
             * Shift Entity - past shifts for employee (Ie. hours worked per week past month etc.), simple loop through each shift, set datastructure to map employee name with hours worked each week
             * Employee Availability Entity - get desired hours, shift category preferences, availability 
             * 
             */


            /* NOTES
AutoGenerate Schedule Data Entity: 
should maybe change the name to Generate Settings, this will have one record right now. on the schedule entity, user can specify which settings they want to use to generate the schedule.

Schedule entity:
	name will be startDate - startDate + 7, will grab from generate settings, will be the entity on which our plugin will be bound. Information for this entity will be displayed on a custom page, allowing for download of excel doc of schedule
	
	
Shifts - small tweaks to be finished but almost done here

EMPLOYEE
	make employee not able to manipulate employee availablity, just view
	need to create a new "switch slots with another employee" which will create the process that employees can switch with another on the schedule without having to bug the scheduler, but still keeping the scheduler aware
	Flesh out the rest of availability change requests
	*/

            //grab every Employee




            
            // Get schedule record
            Entity schedule = (Entity)context.InputParameters["Target"];

            //TODO: add lookup to settings entity on Schedule
            EntityReference settingsRef = schedule.GetAttributeValue<EntityReference>("ljh_autogeneratescheduledata");

            Entity settings = service.Retrieve("ljh_autogeneratescheduledata", settingsRef.Id, new ColumnSet(true));//TODO: filter later

            //TODO: create a field mapping or something so that the settings lowest child has a link to the days and the settings entities




            // Step 1: Get all position types
            var positionTypes = service.RetrieveMultiple(new QueryExpression("ljh_positiontype")
            {
                ColumnSet = new ColumnSet("ljh_name")
            });

            Dictionary<string, Queue<EntityReference>> roleQueues = new Dictionary<string, Queue<EntityReference>>();

            foreach (var position in positionTypes.Entities)
            {
                string roleName = position.GetAttributeValue<string>("ljh_name");

                // Step 2: Get all employees who match this role
                QueryExpression employeeQuery = new QueryExpression("ljh_employee")
                {
                    ColumnSet = new ColumnSet("ljh_employeeid", "ljh_totalhoursworked", "ljh_lastassigneddate")
                };
                employeeQuery.Criteria.AddCondition("ljh_positiontype", ConditionOperator.Equal, position.Id);

                var employees = service.RetrieveMultiple(employeeQuery).Entities;

                List<(Tuple<int, double, DateTime, int>, EntityReference)> sortedList = new();

                foreach (var emp in employees)
                {
                    int assignedThisSchedule = 0; // You’d count from current draft schedule
                    double totalHoursWorked = emp.GetAttributeValue<decimal?>("ljh_totalhoursworked") ?? 0;
                    DateTime lastAssignedDate = emp.GetAttributeValue<DateTime?>("ljh_lastassigneddate") ?? DateTime.MinValue;

                    int preferenceScore = CalculateShiftPreferenceScore(service, emp.Id, startDate);

                    sortedList.Add((Tuple.Create(assignedThisSchedule, totalHoursWorked, lastAssignedDate, -preferenceScore),
                                    emp.ToEntityReference()));
                }

                var ranked = sortedList.OrderBy(x => x.Item1).Select(x => x.Item2);
                roleQueues[roleName] = new Queue<EntityReference>(ranked);
            }

            // roleQueues now contains the employee queues per role, sorted by logic
            // Continue with assigning shifts using these queues...
        
      


        }

private int CalculateShiftPreferenceScore(IOrganizationService service, Guid employeeId, DateTime scheduleStart)
{
    int score = 0;
    // Implement lookup logic: pull shift preferences via N:N and evaluate against planned schedule
    // Example: Retrieve ljh_shiftpreference where ljh_employee == employeeId
    return score;
}

    }

    
}
