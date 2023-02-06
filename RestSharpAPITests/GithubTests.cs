using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace RestSharpAPITests
{
    public class GithubTests
    {
        private RestClient client;
        private RestRequest request;
        private RestResponse response;
        private const string baseUrl = "https://api.github.com";
        private const string partialUrl = "/repos/{user}/{repoName}/issues";
        private const string username = "";
        private string password = "";

        [SetUp]
        public void Setup()
        {
            this.client = new RestClient(baseUrl);
            this.client.Authenticator = new HttpBasicAuthenticator(username, password);
        }

        //Issues
        [Test]
        public void Test_Get_Issue_By_Number() 
        {
            this.request = new RestRequest($"{partialUrl}/1", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");

            Issue issue = JsonSerializer.Deserialize<Issue>(this.response.Content);

            Assert.That(issue.title, Is.EqualTo("PATCH Issue with labels"));
            Assert.That(issue.number, Is.EqualTo(1));
        }

        [Test]
        public void Test_Get_Invalid_Issue_By_Number()
        {
            this.request = new RestRequest($"{partialUrl}/0", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Http status property");
        }

        [Test]
        // ALWAYS ISSUE 15, COMMENT WITH ID 1416802299
        public void Test_Get_Comment_By_Id()
        {
            this.request = new RestRequest($"{partialUrl}/comments/1416802299", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");

            Comment comment = JsonSerializer.Deserialize<Comment>(this.response.Content);

            Assert.That(comment.id, Is.EqualTo(1416802299));
            Assert.That(comment.body, Is.EqualTo("NEW COMMENT WITH POST"));
        }

        [Test]
        public void Test_Get_Invalid_Comment_By_Id()
        {
            this.request = new RestRequest($"{partialUrl}/comments/1409454100", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Http status property");
        }

        [Test]
        public void Test_Get_All_Comments_For_An_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/1/comments", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
            Assert.That(this.response.ContentType, Is.EqualTo("application/json"));

            List<Comment> comments = JsonSerializer.Deserialize<List<Comment>>(this.response.Content);

            Assert.That(comments.Count, Is.EqualTo(1));

            foreach (var c in comments)
            {
                Console.WriteLine(c.id);
                Console.WriteLine(c.body);
                Assert.That(c.body, Is.EqualTo("PATCH MODIFY AN EXISTING COMMENT"));
            }
        }

        [Test]
        public void Test_Get_Empty_Comments()
        {
            this.request = new RestRequest($"{partialUrl}/6/comments", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
            Assert.That(this.response.ContentType, Is.EqualTo("application/json"));

            List<Comment> comments = JsonSerializer.Deserialize<List<Comment>>(this.response.Content);

            Assert.That(comments.Count, Is.EqualTo(0));            
        }

        [Test]
        public void Test_Get_All_Existing_Labels_For_An_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/1/labels", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
            Assert.That(this.response.ContentType, Is.EqualTo("application/json"));

            List<Label> labels = JsonSerializer.Deserialize<List<Label>>(this.response.Content);

            Assert.That(labels.Count, Is.EqualTo(2));
            Assert.That(labels[0].name, Is.EqualTo("PATCHbug"));
            Assert.That(labels[1].name, Is.EqualTo("PATCHpriority:critcal"));


        }

        [Test]
        public void Test_Get_Empty_Labels()
        {
            this.request = new RestRequest($"{partialUrl}/3/labels", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
            Assert.That(this.response.ContentType, Is.EqualTo("application/json"));

            List<Label> labels = JsonSerializer.Deserialize<List<Label>>(this.response.Content);

            Assert.That(labels.Count, Is.EqualTo(0));
           }

        [Test]
        public void Test_Get_All_Issues()
        {
            this.request = new RestRequest($"{partialUrl}", Method.Get);

            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
            Assert.That(this.response.ContentType, Is.EqualTo("application/json"));

            List<Issue> issues = JsonSerializer.Deserialize<List<Issue>>(this.response.Content);

            //1st page
            Assert.That(issues.Count, Is.EqualTo(30));
        }

        [Test]
        public void Test_Create_New_Issue()
        {
            this.request = new RestRequest(partialUrl, Method.Post);

            var issueBody = new
            {
                title = "RestSharp api test",
                body = "some body for an issue",
                labels = new string[] { "bug", "critical", "release" }
            };

            request.AddBody(issueBody);

            this.response = this.client.Execute(this.request);
            Issue issue = JsonSerializer.Deserialize<Issue>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Http status property");

            Assert.That(issue.number, Is.GreaterThan(0));
            Assert.That(issue.title, Is.EqualTo(issueBody.title));
            Assert.That(issue.body, Is.EqualTo(issueBody.body));
        }

        [Test]
        public void Test_Create_Invalid_Issue()
        {
            this.request = new RestRequest(partialUrl, Method.Post);

            var issueBody = new
            {
                title = "",
            };

            request.AddBody(issueBody);

            this.response = this.client.Execute(this.request);
            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity), "Http status property");
        }

        [Test]
        public void Test_Create_Issue_No_Authentication()
        {
            this.request = new RestRequest(partialUrl, Method.Post);

            var issueBody = new
            {
                title = "RestSharp api test",
                body = "some body for an issue",
                labels = new string[] { "bug", "critical", "release" }
            };

            request.AddBody(issueBody);

            RestClient clientNoAuth = new RestClient(baseUrl);

            this.response = clientNoAuth.Execute(this.request);
            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Http status property");
        }

        [Test]
        public void Test_Create_New_Comment_For_An_Existing_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/15/comments", Method.Post);

            var commentBody = new
            {
                body = "NEW COMMENT WITH POST RESTSHARP"
            };

            this.request.AddBody(commentBody);

            this.response = this.client.Execute(this.request);
            Comment comment = JsonSerializer.Deserialize<Comment>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Http status property");

            Assert.That(comment.body, Is.EqualTo(commentBody.body));
        }

        [Test]
        public void Test_Create_Issue_With_Labels()
        {
            this.request = new RestRequest(partialUrl, Method.Post);

            var issueBody = new
            {
                title = "NEW COMMENT WITH POST RESTSHARP",
                labels = new string[] { "bug", "critical", "release" }
            };

            this.request.AddBody(issueBody);

            this.response = this.client.Execute(this.request);
            Issue issue = JsonSerializer.Deserialize<Issue>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Http status property");

            string expectedLabelName  = issue.labels[0].name;
            string actualLabelName = issueBody.labels[0];
            string expectedLabeName2 = issue.labels[1].name;
           string actualLabelName2 = issueBody.labels[1];

            Assert.That(expectedLabelName, Is.EqualTo(actualLabelName));
            Assert.That(expectedLabeName2, Is.EqualTo(actualLabelName2));
        }

        [Test]
        public void Test_Modify_An_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/8", Method.Patch);

            var issueBody = new
            {
                title = "PATCH Issue with labels",
                labels = new string[] { "PATCHbug", "PATCHpriority:critcal" }
            };

            this.request.AddBody(issueBody);

            this.response = this.client.Execute(this.request);
            Issue issue = JsonSerializer.Deserialize<Issue>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");

            Assert.That(issue.title, Is.EqualTo(issueBody.title));
            Assert.That(issue.labels[0].name, Is.EqualTo(issueBody.labels[0]));
            Assert.That(issue.labels[1].name, Is.EqualTo(issueBody.labels[1]));
        }

        [Test]
        public void Test_Modify_An_Issue_Unauthenticated()
        {
            this.request = new RestRequest($"{partialUrl}/8", Method.Patch);

            var issueBody = new
            {
                title = "PATCH Issue with labels",
            };

            this.request.AddBody(issueBody);

            RestClient clientNoAuth = new RestClient(baseUrl);

            this.response = clientNoAuth.Execute(this.request);
            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), "Http status property");
        }

        [Test]
        public void Test_Modify_An_Invalid_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/0", Method.Patch);

            Console.WriteLine();
            var issueBody = new
            {
                title = "PATCH Issue with labels",
                labels = new string[] { "PATCHbug", "PATCHpriority:critcal" }
            };

            this.request.AddBody(issueBody);
            this.response = this.client.Execute(this.request);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Http status property");
        }

        [Test]
        public void Test_Close_An_Issue()
        {
            this.request = new RestRequest($"{partialUrl}/8", Method.Patch);

            var issueBody = new
            {
                state = "closed"
            };

            this.request.AddBody(issueBody);

            this.response = this.client.Execute(this.request);
            Issue issue = JsonSerializer.Deserialize<Issue>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");

            Assert.That(issue.state, Is.EqualTo(issueBody.state));
        }

        [Test]
        public void Test_Modify_An_Existing_Comment()
        {
            this.request = new RestRequest($"{partialUrl}/comments/1409454187", Method.Patch);

            var commentBody = new
            {
                body = "PATCH MODIFY AN EXISTING COMMENT"
            };

            this.request.AddBody(commentBody);

            this.response = this.client.Execute(this.request);
            Comment comment = JsonSerializer.Deserialize<Comment>(this.response.Content);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");

            Assert.That(comment.body, Is.EqualTo(commentBody.body));
        }


        [Test]
        public void Test_Delete_An_Existing_Comment()
        {
            Comment commentId = getExistingComment();

            this.request = new RestRequest($"{partialUrl}/comments/" + commentId, Method.Delete);

            Assert.That(this.response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Http status property");
        }

        private Comment getExistingComment()
        {
            request = new RestRequest($"{partialUrl}/15/comments", Method.Get);

            response = client.Execute(request);

            List<Comment> comments = JsonSerializer.Deserialize<List<Comment>>(response.Content);

            return comments[0];      
        }
    }
}
