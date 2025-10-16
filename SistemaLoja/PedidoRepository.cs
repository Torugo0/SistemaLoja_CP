using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class PedidoRepository
{
    // EXERCÍCIO 7: Criar pedido com itens (transação)
    public void CriarPedido(Pedido pedido, List<PedidoItem> itens)
    {
        if (itens == null || itens.Count == 0)
            throw new ArgumentException("O pedido precisa ter pelo menos um item.");

        // Calcula o total a partir dos itens
        decimal total = itens.Sum(i => i.PrecoUnitario * i.Quantidade);
        pedido.ValorTotal = total;
        pedido.DataPedido = pedido.DataPedido == default ? DateTime.Now : pedido.DataPedido;

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            // Inicia a transação
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Inserir pedido e obter ID
                    string sqlPedido = @"INSERT INTO Pedidos (ClienteId, DataPedido, ValorTotal)
                                         OUTPUT INSERTED.Id
                                         VALUES (@ClienteId, @DataPedido, @ValorTotal)";

                    int pedidoId;
                    using (SqlCommand cmd = new SqlCommand(sqlPedido, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ClienteId", pedido.ClienteId);
                        cmd.Parameters.AddWithValue("@DataPedido", pedido.DataPedido);
                        cmd.Parameters.AddWithValue("@ValorTotal", pedido.ValorTotal);

                        pedidoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2. Inserir itens do pedido
                    string sqlItem = @"INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
                                       VALUES (@PedidoId, @ProdutoId, @Quantidade, @PrecoUnitario)";

                    // 3. Atualizar estoque (com checagem)
                    string sqlEstoque = @"UPDATE Produtos
                                          SET Estoque = Estoque - @Qtd
                                          WHERE Id = @ProdutoId AND Estoque >= @Qtd";

                    foreach (var item in itens)
                    {
                        using (SqlCommand cmdItem = new SqlCommand(sqlItem, conn, transaction))
                        {
                            cmdItem.Parameters.AddWithValue("@PedidoId", pedidoId);
                            cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                            cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                            cmdItem.Parameters.AddWithValue("@PrecoUnitario", item.PrecoUnitario);
                            cmdItem.ExecuteNonQuery();
                        }

                        using (SqlCommand cmdEstoque = new SqlCommand(sqlEstoque, conn, transaction))
                        {
                            cmdEstoque.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                            cmdEstoque.Parameters.AddWithValue("@Qtd", item.Quantidade);
                            int afetadas = cmdEstoque.ExecuteNonQuery();

                            if (afetadas == 0)
                                throw new InvalidOperationException($"Estoque insuficiente para o produto {item.ProdutoId}.");
                        }
                    }

                    // Commit
                    transaction.Commit();
                    Console.WriteLine($"✅ Pedido {pedidoId} criado com sucesso! Total: {pedido.ValorTotal:C2}");
                }
                catch
                {
                    // Rollback em caso de erro
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    // EXERCÍCIO 8: Listar pedidos de um cliente
    public void ListarPedidosCliente(int clienteId)
    {
        string sql = @"SELECT Id, DataPedido, ValorTotal
                       FROM Pedidos
                       WHERE ClienteId = @ClienteId
                       ORDER BY DataPedido DESC";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== PEDIDOS DO CLIENTE {clienteId} ===");
                    while (reader.Read())
                    {
                        Console.WriteLine(
                            $"Id: {reader.GetInt32(0)} | Data: {reader.GetDateTime(1):dd/MM/yyyy HH:mm} | Total: {reader.GetDecimal(2):C2}");
                    }
                }
            }
        }
    }

    // EXERCÍCIO 9: Obter detalhes completos de um pedido
    public void ObterDetalhesPedido(int pedidoId)
    {
        // Mostra cabeçalho do pedido
        const string sqlCab = @"SELECT p.Id, p.ClienteId, p.DataPedido, p.ValorTotal
                                FROM Pedidos p
                                WHERE p.Id = @PedidoId";

        const string sqlItens = @"
            SELECT 
                pi.Id,
                pi.ProdutoId,
                pi.Quantidade,
                pi.PrecoUnitario,
                pr.Nome as NomeProduto,
                (pi.Quantidade * pi.PrecoUnitario) as Subtotal
            FROM PedidoItens pi
            INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
            WHERE pi.PedidoId = @PedidoId
            ORDER BY pi.Id";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmdCab = new SqlCommand(sqlCab, conn))
            {
                cmdCab.Parameters.AddWithValue("@PedidoId", pedidoId);
                using (SqlDataReader r = cmdCab.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        Console.WriteLine("⚠️ Pedido não encontrado.");
                        return;
                    }

                    Console.WriteLine($"\n=== DETALHES DO PEDIDO #{r.GetInt32(0)} ===");
                    Console.WriteLine($"ClienteId: {r.GetInt32(1)}");
                    Console.WriteLine($"Data:      {r.GetDateTime(2):dd/MM/yyyy HH:mm}");
                    Console.WriteLine($"Total:     {r.GetDecimal(3):C2}");
                }
            }

            using (SqlCommand cmdItens = new SqlCommand(sqlItens, conn))
            {
                cmdItens.Parameters.AddWithValue("@PedidoId", pedidoId);
                using (SqlDataReader it = cmdItens.ExecuteReader())
                {
                    Console.WriteLine("\nItens:");
                    while (it.Read())
                    {
                        Console.WriteLine(
                            $"- ItemId: {it.GetInt32(0)} | ProdutoId: {it.GetInt32(1)} | Nome: {it.GetString(4)} | Qtd: {it.GetInt32(2)} | " +
                            $"Preço: {it.GetDecimal(3):C2} | Subtotal: {it.GetDecimal(5):C2}");
                    }
                }
            }
        }
    }

    // DESAFIO 3: Total de vendas por período
    public void TotalVendasPorPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        const string sql = @"SELECT SUM(ValorTotal)
                             FROM Pedidos
                             WHERE DataPedido >= @Ini AND DataPedido < @Fim";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ini", dataInicio);
                cmd.Parameters.AddWithValue("@Fim", dataFim);

                object result = cmd.ExecuteScalar();
                decimal total = (result == DBNull.Value || result == null) ? 0m : Convert.ToDecimal(result);

                Console.WriteLine($"\n💰 Total de vendas entre {dataInicio:dd/MM/yyyy} e {dataFim:dd/MM/yyyy}: {total:C2}");
            }
        }
    }
}