namespace Neo4j.Binary.Tester
{
    using System;
    using System.Linq;
    using Neo4jClient;
    using Neo4jNdpClient;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const string cypher = "MATCH n RETURN count(n) as Items";
            var graphClient = new GraphClient(new Uri("http://localhost:9494/db/data"));
            graphClient.Connect();

            var client = new Neo4jNdpClient(new Uri("http://localhost.:7687"));
            client.Connect();


            var query = graphClient.Cypher.Match("n").Return(n => n.Count());

            var start = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            { var x = query.Results.Single();}
            var took = DateTime.Now - start;
            

            var startNdp = DateTime.Now;
            for (int i = 0; i < 1000; i++)
                client.ExecuteCypher(cypher);
            var tookNdp = DateTime.Now - startNdp;

            Console.WriteLine($"REST: {took.TotalMilliseconds}ms");
            Console.WriteLine($"NDP: {tookNdp.TotalMilliseconds}ms");

            //            client.ExecuteCypher("match (n:Movie) where n.released = 2012 return n.title as Title, n.released as Released", null);
            //            client.ExecuteCypher("match (n:Movie) where n.released > 2000 return n.title as Title, n.released as Released", null);
            //Console.WriteLine(response);

            //            string updateCypher = "match n where n.title = 'Cloud Atlas' set n.released = 2020";
            //            client.ExecuteCypher(updateCypher);

            //            string return1Cypher = "return 1";
            //            client.ExecuteCypher(return1Cypher);


            //            client.ExecuteCypher("match (n:Movie) where n.released = {relParam} return n.title as Title, n.released as Released", new Dictionary<string, dynamic> { { "relParam", 2003 } });


            Console.WriteLine("ENTER exits");
            Console.ReadLine();
        }
    }
}