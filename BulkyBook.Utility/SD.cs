using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public static class SD
    {
        public const string Role_User_Indi = "Individual";
        public const string Role_User_Comp = "Company";
        public const string Role_User_Admin = "Admin";
        public const string Role_User_Employee = "Employee";

        public const string StatusPending = "Pending"; // the initial status => order created
        public const string StatusApproved = "Approved"; // When Payment Is Approved   
        public const string StatusInProcess = "Processing"; // when Admin  are processing the order
        public const string StatusShipped = "Shipped"; // after admin processing => last status 
        public const string StatusRefunded = "Refunded";
        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending"; // the initial payment status 
        public const string PaymentStatusApproved = "Approved"; // payment is done
        public const string PaymentStatusDelayPayment = "ApprovedForDelayPayment"; // for company account
        public const string PaymentStatusRejected = "Rejected ";


    }
}
