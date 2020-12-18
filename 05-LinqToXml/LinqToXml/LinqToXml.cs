using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace LinqToXml
{
    public static class LinqToXml
    {
        /// <summary>
        /// Creates hierarchical data grouped by category
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation (refer to CreateHierarchySourceFile.xml in Resources)</param>
        /// <returns>Xml representation (refer to CreateHierarchyResultFile.xml in Resources)</returns>
        public static string CreateHierarchy(string xmlRepresentation)
        {
            return
                new XElement("Root",
                    from data in XDocument.Parse(xmlRepresentation).Root.Elements("Data")
                    group data by (string)data.Element("Category") into gropedData
                    select new XElement("Group",
                        new XAttribute("ID", gropedData.Key),
                        from g in gropedData
                        select new XElement("Data",
                            g.Element("Quantity"),
                            g.Element("Price")
                        )
                    )
                )
                .ToString();
        }

        /// <summary>
        /// Get list of orders numbers (where shipping state is NY) from xml representation
        /// </summary>
        /// <param name="xmlRepresentation">Orders xml representation (refer to PurchaseOrdersSourceFile.xml in Resources)</param>
        /// <returns>Concatenated orders numbers</returns>
        /// <example>
        /// 99301,99189,99110
        /// </example>
        public static string GetPurchaseOrders(string xmlRepresentation)
        {
            XNamespace aw = "http://www.adventure-works.com";
            var items = from data in XDocument.Parse(xmlRepresentation).Element(aw+"PurchaseOrders").Elements(aw+"PurchaseOrder")
                            from el in data.Elements(aw+"Address")
                                where el.Attribute(aw+"Type").Value == "Shipping" && el.Element(aw+"State").Value == "NY"
                        select data.Attribute(aw+"PurchaseOrderNumber").Value.ToString();

            return String.Join(",", items);
        }

        /// <summary>
        /// Reads csv representation and creates appropriate xml representation
        /// </summary>
        /// <param name="customers">Csv customers representation (refer to XmlFromCsvSourceFile.csv in Resources)</param>
        /// <returns>Xml customers representation (refer to XmlFromCsvResultFile.xml in Resources)</returns>
        public static string ReadCustomersFromCsv(string customers)
        {
            XElement cust = new XElement("Root",
                
                from str in customers.Split(
                    new[] {Environment.NewLine },
                    StringSplitOptions.None
                )

                let fields = str.Split(',')
                
                select new XElement("Customer",
                    new XAttribute("CustomerID", fields[0]),
                    new XElement("CompanyName", fields[1]),
                    new XElement("ContactName", fields[2]),
                    new XElement("ContactTitle", fields[3]),
                    new XElement("Phone", fields[4]),
                    new XElement("FullAddress",
                        new XElement("Address", fields[5]),
                        new XElement("City", fields[6]),
                        new XElement("Region", fields[7]),
                        new XElement("PostalCode", fields[8]),
                        new XElement("Country", fields[9])
                    )
                )
            );

            return cust.ToString();
        }

        /// <summary>
        /// Gets recursive concatenation of elements
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation of document with Sentence, Word and Punctuation elements. (refer to ConcatenationStringSource.xml in Resources)</param>
        /// <returns>Concatenation of all this element values.</returns>
        public static string GetConcatenationString(string xmlRepresentation)
        {
            var items = from data in XDocument.Parse(xmlRepresentation).Element("Document").Elements("Sentence")
                        from el in data.Elements()
                        select el.Value;


            return String.Join("", items);
        }

        /// <summary>
        /// Replaces all "customer" elements with "contact" elements with the same childs
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with customers (refer to ReplaceCustomersWithContactsSource.xml in Resources)</param>
        /// <returns>Xml representation with contacts (refer to ReplaceCustomersWithContactsResult.xml in Resources)</returns>
        public static string ReplaceAllCustomersWithContacts(string xmlRepresentation)
        {
            XDocument doc = XDocument.Parse(xmlRepresentation);

            doc.Element("Document").ReplaceAll(
                from el in doc.Element("Document").Elements("customer")
                select new XElement("contact", el.Elements())
            );

            return doc.ToString();
        }

        /// <summary>
        /// Finds all ids for channels with 2 or more subscribers and mark the "DELETE" comment
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with channels (refer to FindAllChannelsIdsSource.xml in Resources)</param>
        /// <returns>Sequence of channels ids</returns>
        public static IEnumerable<int> FindChannelsIds(string xmlRepresentation)
        {
            return from channel in XDocument.Parse(xmlRepresentation).Element("service").Elements("channel")
                   where channel.Elements("subscriber").Count() >= 2
                        from node in channel.Nodes().OfType<XComment>()
                        where node.Value == "DELETE"
                   select Int32.Parse(channel.Attribute("id").Value);
        }

        /// <summary>
        /// Sort customers in docement by Country and City
        /// </summary>
        /// <param name="xmlRepresentation">Customers xml representation (refer to GeneralCustomersSourceFile.xml in Resources)</param>
        /// <returns>Sorted customers representation (refer to GeneralCustomersResultFile.xml in Resources)</returns>
        public static string SortCustomers(string xmlRepresentation)
        {
            var items = XDocument.Parse(xmlRepresentation).Root.Elements("Customers")
                .OrderBy(c => c.Element("FullAddress").Element("Country").Value)
                .ThenBy(c => c.Element("FullAddress").Element("City").Value);

            return new XElement("Root", items).ToString();
        }

        /// <summary>
        /// Gets XElement flatten string representation to save memory
        /// </summary>
        /// <param name="xmlRepresentation">XElement object</param>
        /// <returns>Flatten string representation</returns>
        /// <example>
        ///     <root><element>something</element></root>
        /// </example>
        public static string GetFlattenString(XElement xmlRepresentation)
        {
            return xmlRepresentation.Elements().ToString();
        }

        /// <summary>
        /// Gets total value of orders by calculating products value
        /// </summary>
        /// <param name="xmlRepresentation">Orders and products xml representation (refer to GeneralOrdersFileSource.xml in Resources)</param>
        /// <returns>Total purchase value</returns>
        public static int GetOrdersValue(string xmlRepresentation)
        {
            Dictionary<string, int> dict =  XDocument.Parse(xmlRepresentation).Root.Element("products").Elements("product")
                                           .ToDictionary(k => k.Attribute("Id").Value, v => Int32.Parse(v.Attribute("Value").Value));

            var items = from order in XDocument.Parse(xmlRepresentation).Root.Element("Orders").Elements("Order")
                        select dict[order.Element("product").Value.ToString()];

            return items.Sum();
        }
    }
}
