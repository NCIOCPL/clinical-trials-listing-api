using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NCI.OCPL.Api.Common.Testing
{
    /// <summary>
    /// Helper tools for loading responses from files and
    /// deserializing XML to objects.
    /// </summary>
    public static class TestingTools
    {
        /// <summary>
        /// Gets a test file from the TestData folder and returns an array of bytes
        /// </summary>
        /// <returns>An array of byte</returns>
        public static byte[] GetTestFileAsBytes(string testFile)
        {

            //Get the path to the file.
            string path = GetPathToTestFile(testFile);

            //Get the bytes
            byte[] contents = File.ReadAllBytes(path);

            return contents;
        }

        /// <summary>
        /// Gets a test file from the TestData folder as a stream
        /// </summary>
        /// <param name="testFile">The name of the testfile</param>
        /// <returns></returns>
        public static Stream GetTestFileAsStream(string testFile)
        {
            //Get the path to the file.
            string path = GetPathToTestFile(testFile);

            //Get the bytes
            Stream contents = File.OpenRead(path);

            return contents;
        }

        /// <summary>
        /// Gets a string as a stream. Useful for when you don't want to maintain a
        /// separate file but have a string which needs to look like it came from one.
        /// </summary>
        /// <param name="dataString">The string to convert.</param>
        /// <returns></returns>
        public static Stream GetStringAsStream(string dataString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(dataString);
            return new MemoryStream(byteArray);
        }

        /// <summary>
        /// Deserializes the contents of a test XML file into an object of type T
        /// </summary>
        /// <param name="testFile">The test XML filename</param>
        /// <remarks>This will not handle any exceptions.</remarks>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeXML<T>(string testFile) where T : class
        {
            // Get the path to the file.
            string filePath = GetPathToTestFile(testFile);

            // Create the serializer
            XmlSerializer serializer = new XmlSerializer(typeof(T), "cde");

            // Load the file and deserialize
            using (FileStream xmlFile = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using (XmlReader xmlReader = XmlReader.Create(filePath))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        /// <summary>
        /// Gets the path to a test file within the testdata folder.
        /// </summary>
        /// <param name="testFile">The test filename</param>
        /// <returns>The full path to the file.</returns>
        public static string GetPathToTestFile(string testFile)
        {
            // Determine where the output folder is that should be the parent for the TestData
            string assmPath = Path.GetDirectoryName(typeof(TestingTools).GetTypeInfo().Assembly.Location);

            // Build a path to the test file
            string path = Path.Combine(new string[] { assmPath, "TestData", testFile });

            return path;
        }

    }


}
