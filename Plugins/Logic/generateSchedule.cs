using Microsoft.Xrm.Sdk;
using System;

namespace Landen.SchedulingApp.Plugins
{
    /// <summary>
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class generateSchedule : PluginBase
    {
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
            
        }
    }
}
