using RulesX.Metadata.Rule;
using System;
using System.Collections.Generic;
using RulesX.Metadata.Rule.Extensions;

namespace RulesX
{
    class Program
    {
        static void Main(string[] args)
        {
            var customer1= new Customer
            {
                CustomerId = 1,
                BirthDate = new DateTime(1989,10,9)
            };

            var customer2 = new Customer
            {
                CustomerId = 2,
                BirthDate = new DateTime(1989, 10, 9)
            };

            var customer3 = new Customer
            {
                CustomerId = 3,
                BirthDate = new DateTime(1989, 10, 9)
            };

            var listOfCustomers= new List<Customer>{customer1, customer2, customer3};

            var rule1 = new Rule<Customer>(OperationCode.Equals, "1", "CustomerId");
            var rule2 = new Rule<Customer>(OperationCode.Equals, "2", "CustomerId");
            var rule3 = new Rule<Customer>(OperationCode.Equals, "10/09/1989", "BirthDate");
            var resultOfOr = rule1.Or(rule2).Evaluate(listOfCustomers);
            var resultOfAnd = rule1.And(rule3).Evaluate(listOfCustomers);
        }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public DateTime? BirthDate { get; set; }
        public Account Account { get; set; }
    }

    public class Account
    {
        public int AccountId { get; set; }
    }
}
