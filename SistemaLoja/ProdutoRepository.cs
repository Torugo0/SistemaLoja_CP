using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class ProdutoRepository
{
    // EXERCÍCIO 1: Listar todos os produtos
    public void ListarTodosProdutos()
    {
        string sql = "SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("\n=== LISTA DE PRODUTOS ===");
                while (reader.Read())
                {
                    Console.WriteLine(
                        $"Id: {reader.GetInt32(0)} | Nome: {reader.GetString(1)} | Preço: {reader.GetDecimal(2):C2} | Estoque: {reader.GetInt32(3)} | CategoriaId: {reader.GetInt32(4)}");
                }
            }
        }
    }

    // EXERCÍCIO 2: Inserir novo produto
    public void InserirProduto(Produto produto)
    {
        string sql = @"INSERT INTO Produtos (Nome, Preco, Estoque, CategoriaId)
                       VALUES (@Nome, @Preco, @Estoque, @CategoriaId)";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", produto.Nome);
                cmd.Parameters.AddWithValue("@Preco", produto.Preco);
                cmd.Parameters.AddWithValue("@Estoque", produto.Estoque);
                cmd.Parameters.AddWithValue("@CategoriaId", produto.CategoriaId);

                cmd.ExecuteNonQuery();
                Console.WriteLine("✅ Produto inserido com sucesso!");
            }
        }
    }

    // EXERCÍCIO 3: Atualizar produto
    public void AtualizarProduto(Produto produto)
    {
        string sql = @"UPDATE Produtos
                       SET Nome = @Nome, Preco = @Preco, Estoque = @Estoque, CategoriaId = @CategoriaId
                       WHERE Id = @Id";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", produto.Nome);
                cmd.Parameters.AddWithValue("@Preco", produto.Preco);
                cmd.Parameters.AddWithValue("@Estoque", produto.Estoque);
                cmd.Parameters.AddWithValue("@CategoriaId", produto.CategoriaId);
                cmd.Parameters.AddWithValue("@Id", produto.Id);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "✅ Produto atualizado!" : "⚠️ Produto não encontrado.");
            }
        }
    }

    // EXERCÍCIO 4: Deletar produto (com verificação de vínculo)
    public void DeletarProduto(int id)
    {
        // Verifica vínculos em itens de pedido
        const string sqlCount = @"SELECT COUNT(1)
                                  FROM PedidoItens
                                  WHERE ProdutoId = @Id";

        const string sqlDelete = @"DELETE FROM Produtos WHERE Id = @Id";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand check = new SqlCommand(sqlCount, conn))
            {
                check.Parameters.AddWithValue("@Id", id);
                int vinculados = Convert.ToInt32(check.ExecuteScalar());

                if (vinculados > 0)
                    throw new InvalidOperationException("Não é possível excluir: há pedidos vinculados a este produto.");

                using (SqlCommand del = new SqlCommand(sqlDelete, conn))
                {
                    del.Parameters.AddWithValue("@Id", id);
                    int rows = del.ExecuteNonQuery();
                    Console.WriteLine(rows > 0 ? "✅ Produto deletado!" : "⚠️ Produto não encontrado.");
                }
            }
        }
    }

    // EXERCÍCIO 5: Buscar produto por ID
    public Produto BuscarPorId(int id)
    {
        string sql = "SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos WHERE Id = @Id";
        Produto produto = null;

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        produto = new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Preco = reader.GetDecimal(2),
                            Estoque = reader.GetInt32(3),
                            CategoriaId = reader.GetInt32(4)
                        };
                    }
                }
            }
        }

        return produto;
    }

    // EXERCÍCIO 6: Listar produtos por categoria
    public void ListarProdutosPorCategoria(int categoriaId)
    {
        string sql = @"
            SELECT p.Id, p.Nome, p.Preco, p.Estoque, c.Nome AS NomeCategoria
            FROM Produtos p
            INNER JOIN Categorias c ON p.CategoriaId = c.Id
            WHERE p.CategoriaId = @CategoriaId
            ORDER BY p.Nome";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\n=== PRODUTOS POR CATEGORIA ===");
                    while (reader.Read())
                    {
                        Console.WriteLine(
                            $"Id: {reader.GetInt32(0)} | Nome: {reader.GetString(1)} | Preço: {reader.GetDecimal(2):C2} | Estoque: {reader.GetInt32(3)} | Categoria: {reader.GetString(4)}");
                    }
                }
            }
        }
    }

    // DESAFIO 1: Estoque baixo
    public void ListarProdutosEstoqueBaixo(int quantidadeMinima)
    {
        string sql = @"SELECT Id, Nome, Preco, Estoque FROM Produtos WHERE Estoque < @Qtd ORDER BY Estoque ASC";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Qtd", quantidadeMinima);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== PRODUTOS COM ESTOQUE < {quantidadeMinima} ===");
                    while (reader.Read())
                    {
                        var estoque = reader.GetInt32(3);
                        var alerta = estoque <= 0 ? "❌" : "⚠️";
                        Console.WriteLine($"{alerta} Id: {reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetDecimal(2):C2} | Estoque: {estoque}");
                    }
                }
            }
        }
    }

    // DESAFIO 2: Buscar por nome (LIKE)
    public void BuscarProdutosPorNome(string termoBusca)
    {
        string sql = @"SELECT Id, Nome, Preco, Estoque FROM Produtos
                       WHERE Nome LIKE @Termo
                       ORDER BY Nome";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Termo", "%" + termoBusca + "%");

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== BUSCA POR NOME: \"{termoBusca}\" ===");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Id: {reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetDecimal(2):C2} | Estoque: {reader.GetInt32(3)}");
                    }
                }
            }
        }
    }
}
