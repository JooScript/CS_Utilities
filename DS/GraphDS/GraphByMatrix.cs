namespace Utils.DS.GraphDS;

public class GraphByMatrix<TVertex>
{
    public enum enGraphDirectionType { Directed, unDirected }
    private int[,] _adjacencyMatrix;
    private Dictionary<TVertex, int> _vertexDictionary;
    private int _numberOfVertices;
    private enGraphDirectionType _GraphDirectionType = enGraphDirectionType.unDirected;

    public GraphByMatrix(List<TVertex> vertices, enGraphDirectionType GraphDirectionType)
    {
        _GraphDirectionType = GraphDirectionType;
        _numberOfVertices = vertices.Count;
        _adjacencyMatrix = new int[_numberOfVertices, _numberOfVertices];
        _vertexDictionary = new Dictionary<TVertex, int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            _vertexDictionary[vertices[i]] = i;
        }
    }

    public void AddEdge(TVertex source, TVertex destination, int weight = 1)
    {
        if (_vertexDictionary.ContainsKey(source) && _vertexDictionary.ContainsKey(destination))
        {
            int sourceIndex = _vertexDictionary[source];
            int destinationIndex = _vertexDictionary[destination];

            _adjacencyMatrix[sourceIndex, destinationIndex] = weight;

            if (_GraphDirectionType == enGraphDirectionType.unDirected)
            {
                _adjacencyMatrix[destinationIndex, sourceIndex] = weight;
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
            int sourceIndex = _vertexDictionary[source];
            int destinationIndex = _vertexDictionary[destination];

            _adjacencyMatrix[sourceIndex, destinationIndex] = 0;
            _adjacencyMatrix[destinationIndex, sourceIndex] = 0;
        }
        else
        {
            throw new Exception("Invalid vertices: One or both vertices do not exist in the graph.");
        }
    }


    public void DisplayGraph(string Message)
    {
        Console.WriteLine("\n" + Message + "\n");
        Console.Write("  ");
        foreach (var vertex in _vertexDictionary.Keys)
        {
            Console.Write(vertex + " ");
        }
        Console.WriteLine();


        foreach (var source in _vertexDictionary)
        {
            Console.Write(source.Key + " ");
            for (int j = 0; j < _numberOfVertices; j++)
            {
                Console.Write(_adjacencyMatrix[source.Value, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public bool IsEdge(TVertex source, TVertex destination)
    {
        if (_vertexDictionary.ContainsKey(source) && _vertexDictionary.ContainsKey(destination))
        {
            int sourceIndex = _vertexDictionary[source];
            int destinationIndex = _vertexDictionary[destination];

            return _adjacencyMatrix[sourceIndex, destinationIndex] > 0;
        }

        return false;
    }

    public int GetInDegree(TVertex vertex)
    {
        int degree = 0;

        if (_vertexDictionary.ContainsKey(vertex))
        {
            int vertexIndex = _vertexDictionary[vertex];

            for (int i = 0; i < _numberOfVertices; i++)
            {
                if (_adjacencyMatrix[i, vertexIndex] > 0)
                    degree++;
            }
        }

        return degree;
    }

    public int GetOutDegree(TVertex vertex)
    {
        int degree = 0; // Initialize the degree count to zero

        // Check if the vertex exists in the map
        if (_vertexDictionary.ContainsKey(vertex))
        {
            int vertexIndex = _vertexDictionary[vertex];

            // Loop through all possible connections (columns) for the given vertex (row)
            for (int i = 0; i < _numberOfVertices; i++)
            {
                // If there's an edge (i.e., weight is greater than 0), increment the degree count
                if (_adjacencyMatrix[vertexIndex, i] > 0)
                    degree++;
            }
        }

        return degree;
    }

}