using System.Xml;
using System.Xml.XPath;
using System.IO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Gallery;
using static DotNetNuke.Modules.Gallery.Utils;
using System;
using System.Collections;

//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2011 by DotNetNuke Corp.

//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//


namespace DotNetNuke.Modules.Gallery
{

    public class GalleryXML
    {

        // XML Filename
        const string xmlFileName = "_metadata.resources";
        private System.Xml.XPath.XPathDocument xml;
        private XPathNavigator xpath;

        private bool bmetaDataExists = false;

        // Determines whether the metadata files exists; allows us to speed up processing
        // if it doesn't
        public bool MetaDataExists
        {
            get
            {
                return bmetaDataExists;
            }
        }

        // Creator function
        public GalleryXML(string Directory)
        {
            string xmlPath = System.Convert.ToString(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));

            // Check for existence of file and build if necessary
            if (!File.Exists(xmlPath))
            {
                CreateMetaDataFile(Directory);
            }

            // Often if xml file gets corrupted exception will fire so now we just recreate the file on exception
            // Find out if metadata file exists; if it does, init some vars
            if (File.Exists(xmlPath))
            {
                bmetaDataExists = true;
                try
                {
                    // Open it
                    xml = new XPathDocument(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
                }
                catch (Exception)
                {
                    // Error... Try deleting it, creating it, and reopening it
                    File.Delete(xmlPath);
                    CreateMetaDataFile(Directory);
                    xml = new XPathDocument(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
                }

                xpath = xml.CreateNavigator();
            }

        }

        // Gets the folder or file ID data for a given filename in a folder
        public int ID(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/id");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return int.Parse(System.Convert.ToString(iterator.Current.Value));
                }
                else
                {
                    return -1;
                }

            }
            else
            {
                return -1;
            }

        }

        // Gets the title data for a given filename is a folder
        public string Title(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/title");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        // Gets the description data for a given filename is a folder
        public string Description(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/description");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        // Gets the category data for a given filename is a folder
        public string Categories(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/categories");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        public string Author(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/author");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        public string Location(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/location");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        public string Client(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/client");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return iterator.Current.Value;
                }
                else
                {
                    return Null.NullString;
                }

            }
            else
            {
                return Null.NullString;
            }

        }

        // Gets the owner data for a given filename is a folder
        public int OwnerID(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/ownerid");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    // William Severance - Added check for empty value and replacement by default -1
                    string s = System.Convert.ToString(iterator.Current.Value);
                    if (!string.IsNullOrEmpty(s))
                    {
                        return int.Parse(s);
                    }
                }
            }
            return -1; //Default OwnerID for non-specified, non-private gallery object
        }

        // Gets the owner data for a given filename is a folder
        public DateTime CreatedDate(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/createddate");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    // Add a try around the date conversion in case it fails
                    try
                    {
                        return XmlConvert.ToDateTime(iterator.Current.Value, XmlDateTimeSerializationMode.Local);
                    }
                    catch
                    {
                        return Null.NullDate;
                    }
                }
                else
                {
                    return Null.NullDate;
                }

            }
            else
            {
                return Null.NullDate;
            }

        }

        // Gets the approved date data for a given filename in a folder
        public DateTime ApprovedDate(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/approveddate");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    // Added a try around the date conversion in case it fails
                    try
                    {
                        return XmlConvert.ToDateTime(iterator.Current.Value, XmlDateTimeSerializationMode.Local);
                    }
                    catch
                    {
                        return DateTime.MaxValue;
                    }
                }
                else
                {
                    return DateTime.MaxValue; // <to be used with approval feature>
                }

            }
            else
            {
                return DateTime.MaxValue;
            }

        }

        // Gets the approved date data for a given filename is a folder
        public double Score(string FileName)
        {

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file

                XPathNodeIterator iterator = default(XPathNodeIterator);

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/file[@name=\"" + FileName + "\"]/score");

                if (iterator.MoveNext())
                {
                    // If we moved, then we retrieved a result
                    return Convert.ToDouble(iterator.Current.Value);
                }
                else
                {
                    return 0;
                }

            }
            else
            {
                return 0;
            }

        }

        public static void DeleteMetaData(string Directory, string Name)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = default(XmlNode);
            XmlNode fileNode = default(XmlNode);

            // Check for existence of file
            if (!File.Exists(System.Convert.ToString(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true))))
            {
                return;
            }

            // Load the XML and init a few items
            xml.Load(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            root = xml.DocumentElement;
            fileNode = xml.SelectSingleNode("/files/file[@name=\"" + Name + "\"]");

            if (!ReferenceEquals(fileNode, null))
            {
                root.RemoveChild(fileNode);
            }

            // Now save the file back so that the updates are saved.
            xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
        }

        /// <summary>
        /// Save the metadata to the XML file
        /// </summary>
        /// <param name="Directory"></param>
        /// <param name="FileName"></param>
        /// <param name="ID"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <param name="Categories"></param>
        /// <param name="Author"></param>
        /// <param name="Location"></param>
        /// <param name="Client"></param>
        /// <param name="OwnerID"></param>
        /// <param name="CreatedDate"></param>
        /// <param name="ApprovedDate"></param>
        /// <param name="Score"></param>
        /// <remarks></remarks>
        public static void SaveMetaData(string Directory, string FileName, int ID, string Title, string Description, string Categories, string Author, string Location, string Client, int OwnerID, DateTime CreatedDate, DateTime ApprovedDate, double Score)
        {

            XmlNode root = default(XmlNode);
            XmlNode fileNode = default(XmlNode);
            XmlNode newNode = default(XmlNode);
            XmlNode subNode = default(XmlNode);
            XmlAttribute attr = default(XmlAttribute);
            XmlDocument xml = new XmlDocument();

            // Check for existence of file and build if necessary
            if (!File.Exists(System.Convert.ToString(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true))))
            {
                CreateMetaDataFile(Directory);
            }

            // Load the XML and init a few items
            xml.Load(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            root = xml.DocumentElement;

            // Regardless of the circumstance, we need to create a new node for the update
            newNode = xml.CreateElement("file");

            attr = xml.CreateAttribute("name");
            attr.Value = FileName;
            newNode.Attributes.Append(attr);

            if (ID <= 0) // New item
            {
                System.Random rd = new System.Random();
                ID = System.Convert.ToInt32(rd.Next());
            }

            subNode = xml.CreateElement("id");
            subNode.InnerText = ID.ToString();
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("title");
            subNode.InnerText = Title;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("description");
            subNode.InnerText = Description;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("categories");
            subNode.InnerText = Categories;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("author");
            subNode.InnerText = Author;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("location");
            subNode.InnerText = Location;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("client");
            subNode.InnerText = Client;
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("ownerid");
            subNode.InnerText = OwnerID.ToString();
            newNode.AppendChild(subNode);

            // Write date in XML format
            subNode = xml.CreateElement("createddate");
            subNode.InnerText = XmlConvert.ToString(CreatedDate, XmlDateTimeSerializationMode.Local); // ISO 8601 date format
            newNode.AppendChild(subNode);

            // Write date in XML format
            subNode = xml.CreateElement("approveddate");
            subNode.InnerText = XmlConvert.ToString(ApprovedDate, XmlDateTimeSerializationMode.Local); // ISO 8601 date format
            newNode.AppendChild(subNode);

            subNode = xml.CreateElement("score");
            subNode.InnerText = Score.ToString();
            newNode.AppendChild(subNode);

            // We now have the complete node to add/update

            // Look for the existence of this node already
            fileNode = xml.SelectSingleNode("/files/file[@name=\"" + FileName + "\"]");

            if (ReferenceEquals(fileNode, null))
            {
                // Node was not found.  Add it
                root.AppendChild(newNode);
            }
            else
            {
                // Node was found.  Need to update instead
                root.ReplaceChild(newNode, fileNode);
            }

            // Now save the file back so that the updates are saved.
            xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
        }

        private static void AddCatetory(string Directory, string Value)
        {

            XmlNode root = default(XmlNode);
            XmlNode catNode = default(XmlNode);
            XmlNode newNode = default(XmlNode);
            XmlNode oldNode = default(XmlNode);
            XmlAttribute attr = default(XmlAttribute);
            XmlDocument xml = new XmlDocument();

            // Check for existence of file and build if necessary
            if (!File.Exists(System.Convert.ToString(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true))))
            {
                CreateMetaDataFile(Directory);
            }

            // Load the XML and init a few items
            xml.Load(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            root = xml.DocumentElement;
            catNode = root.SelectSingleNode("categories");

            //Regardless of the circumstance, we need to create a new node for the update
            newNode = xml.CreateElement("category");

            attr = xml.CreateAttribute("value");
            attr.Value = Value;
            newNode.Attributes.Append(attr);

            // We now have the complete node to add/update

            // Look for the existence of this node already
            oldNode = root.SelectSingleNode("/categories/category[@value='" + Value + "']");

            if (ReferenceEquals(oldNode, null))
            {
                // Node was not found.  Add it
                catNode.AppendChild(newNode);
            }
            else
            {
                // Node was found.  Need to update instead
                catNode.ReplaceChild(newNode, oldNode);
            }

            // Now save the file back so that the updates are saved.
            xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
        }

        public static void AddVote(string Directory, Vote vote)
        {

            XmlNode root = default(XmlNode);
            XmlNode fileVotesNode = default(XmlNode);
            XmlNode newNode = default(XmlNode);
            XmlAttribute attrName = default(XmlAttribute);
            XmlAttribute attrUserID = default(XmlAttribute);
            XmlAttribute attrScore = default(XmlAttribute);
            XmlAttribute attrCreatedDate = default(XmlAttribute);
            XmlDocument xml = new XmlDocument();

            // Load the XML and init a few items
            xml.Load(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            root = xml.DocumentElement;
            // JIMJ declare it here, until we are finally are ready for it
            //Dim fileNode As XmlNode
            //fileNode = root.SelectSingleNode("files")

            if (ReferenceEquals(root.SelectSingleNode("votes[@name='" + vote.FileName + "']"), null))
            {
                XmlNode newVotesNode = xml.CreateElement("votes");
                attrName = xml.CreateAttribute("name");
                attrName.Value = vote.FileName;
                newVotesNode.Attributes.Append(attrName);
                root.AppendChild(newVotesNode);

                fileVotesNode = newVotesNode;
            }
            else
            {
                fileVotesNode = root.SelectSingleNode("votes[@name='" + vote.FileName + "']");
            }

            //fileVotesNode = fileNode.SelectSingleNode("files/votes[@name='" & vote.FileName & "']")

            //Dim xml As New XmlDocument
            //Dim rootNode, newNode As XmlNode

            //' Create the declaration node and root node and add them
            //newNode = xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", Nothing)
            //rootNode = xml.CreateElement("files")

            //xml.AppendChild(newNode)
            //xml.AppendChild(rootNode)

            // create a new node for the update
            newNode = xml.CreateElement("vote");

            attrUserID = xml.CreateAttribute("userid");
            attrUserID.Value = vote.UserID.ToString();
            newNode.Attributes.Append(attrUserID);

            attrScore = xml.CreateAttribute("score");
            attrScore.Value = vote.Score.ToString();
            newNode.Attributes.Append(attrScore);

            attrCreatedDate = xml.CreateAttribute("createddate");
            attrCreatedDate.Value = XmlConvert.ToString(vote.CreatedDate, XmlDateTimeSerializationMode.Local); // ISO 8601 date format
            newNode.Attributes.Append(attrCreatedDate);

            newNode.InnerText = vote.Comment;

            //newNode.Value = vote.Comment

            // We now have the complete node to add/update
            fileVotesNode.AppendChild(newNode);

            // Now save the file back so that the updates are saved.
            xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));

        }

        public ArrayList GetVotings(string FileName)
        {
            // Declare so that it always exists
            ArrayList voteList = new ArrayList();

            if (bmetaDataExists)
            {
                // Only run this if we have an XML file
                XPathNodeIterator iterator = default(XPathNodeIterator);
                int i = 0;

                // Get the iterator object for given Xpath search
                iterator = xpath.Select("/files/votes[@name=\"" + FileName + "\"]");
                if (iterator.MoveNext())
                {
                    XPathNodeIterator childIterator = iterator.Current.SelectChildren("vote", "");

                    for (i = 0; i <= childIterator.Count; i++) //.Current.SelectChildren(XPath.XPathNodeType.Element .Count - 1
                    {
                        if (childIterator.MoveNext())
                        {
                            //Dim voteIterator As xpath.XPathNodeIterator = childIterator.Current.SelectChildren("vote", "")
                            XPathNavigator voteNav = childIterator.Current;

                            Vote vote = new Vote();
                            vote.UserID = int.Parse(System.Convert.ToString(GetValue(voteNav.GetAttribute("userid", ""), "0")));
                            vote.Score = short.Parse(System.Convert.ToString(GetValue(voteNav.GetAttribute("score", ""), "0")));
                            try
                            {
                                vote.CreatedDate = XmlConvert.ToDateTime(GetValue(voteNav.GetAttribute("createddate", ""), XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local)), XmlDateTimeSerializationMode.Local);
                            }
                            catch (Exception)
                            {
                                // Couldn't convert date, use nothing
                                vote.CreatedDate = Null.NullDate;
                            }
                            vote.Comment = GetValue(voteNav.Value, "");
                            voteList.Add(vote);
                        }
                    }
                }
            }

            // Return a new'd votelist even if empty
            return voteList;
        }

        //William Severance - Does not appear to be called from anywhere in project. Leave as is for now.
        private static void AddGalleryOwner(string Directory, string Value)
        {
            XmlNode root = default(XmlNode);
            XmlNode newNode = default(XmlNode);
            XmlNode oldNode = default(XmlNode);
            XmlDocument xml = new XmlDocument();

            // Check for existence of file and build if necessary
            if (!File.Exists(System.Convert.ToString(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true))))
            {
                CreateMetaDataFile(Directory);
            }

            // Load the XML and init a few items
            xml.Load(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            root = xml.DocumentElement;

            //Regardless of the circumstance, we need to create a new node for the update
            newNode = xml.CreateElement("owner");
            newNode.InnerText = Value;

            // We now have the complete node to add/update

            // Look for the existence of this node already
            oldNode = root.SelectSingleNode("owner");

            if (ReferenceEquals(oldNode, null))
            {
                // Node was not found.  Add it
                root.AppendChild(newNode);
            }
            else
            {
                // Node was found.  Need to update instead
                root.ReplaceChild(newNode, oldNode);
            }

            // Now save the file back so that the updates are saved.
            xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
        }

        private static void CreateMetaDataFile(string Directory)
        {
            // Creates new XML metadata file for a particular folder
            XmlDocument xml = new XmlDocument();
            XmlNode rootNode = default(XmlNode);
            XmlNode newNode = default(XmlNode);

            // Create the declaration node and root node and add them
            newNode = xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", null);
            rootNode = xml.CreateElement("files");

            xml.AppendChild(newNode);
            xml.AppendChild(rootNode);

            // Add error logging around the save as that is where it fails
            // When the directory/file doesn't have permissions
            // Now, save the file
            try
            {
                xml.Save(BuildPath(new string[2] { Directory, xmlFileName }, "\\", false, true));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw;
            }
        }

    }

}