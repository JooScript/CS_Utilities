namespace Utils.DS.GraphDS;

public class GraphByList<TVertex>
{
    public enum enGraphDirectionType { Directed, unDirected }
    private Dictionary<TVertex, List<Tuple<TVertex, int>>> _adjacencyList;
    private Dictionary<TVertex, int> _vertexDictionary;
    private int _numberOfVertices;
    private enGraphDirectionType _GraphDirectionType = enGraphDirectionType.unDirected;

    public GraphByList(List<TVertex> vertices, enGraphDirectionType GraphDirectionType)
    {
        _GraphDirectionType = GraphDirectionType;
        _numberOfVertices = vertices.Count;
        _adjacencyList = new Dictionary<TVertex, List<Tuple<TVertex, int>>>();
        _vertexDictionary = new Dictionary<TVertex, int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            _vertexDictionary[vertices[i]] = i;
            _adjacencyList[vertices[i]] = new List<Tuple<TVertex, int>>();  // Initialize an empty list for each vertex
        }
    }


    public void AddEdge(TVertex source, TVertex destination, int weight)
    {
        if (_vertexDictionary.ContainsKey(source) && _vertexDictionary.ContainsKey(destination))
        {

            _adjacencyList[source].Add(new Tuple<TVertex, int>(destination, weight));

            if (_GraphDirectionType == enGraphDirectionType.unDirected)
            {
                _adjacencyList[destination].Add(new Tuple<TVertex, int>(source, weight));
            }
        }
        else
        {
            throw new Exception("Invalid vertices: One or both vertices do not exist in the graph.");
        }
    }

    public void RemoveEdge(TVertex source, TVertex destination)
    {
        if (_vertexDictionary.ContainsKey(source) && _vertexDictionary.ContainsKey(destination))
        {
            _adjacencyList[source].RemoveAll(edge => AreEqual(edge.Item1, destination));

            if (_GraphDirectionType == enGraphDirectionType.unDirected)
            {
                _adjacencyList[destination].RemoveAll(edge => AreEqual(edge.Item1, source));
            }
        }
        else
        {
            throw new Exception("Invalid vertices: One or both vertices do not exist in the graph.");
        }
    }

    public void DisplayGraph(string Message)
    {
        Console.WriteLine("\n" + Message + "\n");

        foreach (var vertex in _adjacencyList)
        {
            Console.Write(vertex.Key + " -> ");

            foreach (var edge in vertex.Value)
            {
                Console.Write($"{edge.Item1}({edge.Item2}) ");
            }
            Console.WriteLine();
        }
    }

    public bool IsEdge(TVertex source, TVertex destination)
    {
        if (_vertexDictionary.ContainsKey(source) && _vertexDictionary.ContainsKey(destination))
        {
            foreach (var edge in _adjacencyList[source])
            {
                if (AreEqual(edge.Item1, destination))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool AreEqual<T>(T a, T b)
    {
        return EqualityComparer<T>.Default.Equals(a, b);
    }

    public int GetInDegree(TVertex vertex)
    {
        int inDegree = 0;

        if (_vertexDictionary.ContainsKey(vertex))
        {
            foreach (var source in _adjacencyList)
            {
                foreach (var edge in source.Value)
                {
                    if (AreEqual(edge.Item1, vertex))
                    {
                        inDegree++;
                    }
                }
            }
        }

        return inDegree;
    }

    public int GetOutDegree(TVertex vertex)
    {
        int outDegree = 0;

        // Check if the vertex exists in the map
        if (_vertexDictionary.ContainsKey(vertex))
        {
            outDegree = _adjacencyList[vertex].Count;
        }

        return outDegree;
    }


}