using System;
using Microsoft.Data.SqlClient;
using SistemaLoja.Lab12_ConexaoSQLServer;

namespace SistemaLoja
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== LAB 12 - CONEXÃO SQL SERVER ===\n");

            var produtoRepo = new ProdutoRepository();
            var pedidoRepo = new PedidoRepository();

            bool continuar = true;

            while (continuar)
            {
                MostrarMenu();
                string opcao = Console.ReadLine();

                try
                {
                    switch (opcao)
                    {
                        case "1":
                            produtoRepo.ListarTodosProdutos();
                            break;

                        case "2":
                            InserirNovoProduto(produtoRepo);
                            break;

                        case "3":
                            AtualizarProdutoExistente(produtoRepo);
                            break;

                        case "4":
                            DeletarProdutoExistente(produtoRepo);
                            break;

                        case "5":
                            ListarPorCategoria(produtoRepo);
                            break;

                        case "6":
                            CriarNovoPedido(pedidoRepo);
                            break;

                        case "7":
                            ListarPedidosDeCliente(pedidoRepo);
                            break;

                        case "8":
                            DetalhesDoPedido(pedidoRepo);
                            break;

                        case "9":
                            ListarCategorias(); // nova opção
                            break;

                        case "0":
                            continuar = false;
                            break;

                        default:
                            Console.WriteLine("Opção inválida!");
                            break;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"\n❌ Erro SQL: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro: {ex.Message}");
                }

                if (continuar)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            Console.WriteLine("\nPrograma finalizado!");
        }

        // Mostra menu principal
        static void MostrarMenu()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║       MENU PRINCIPAL               ║");
            Console.WriteLine("╠════════════════════════════════════╣");
            Console.WriteLine("║  PRODUTOS                          ║");
            Console.WriteLine("║  1 - Listar todos os produtos      ║");
            Console.WriteLine("║  2 - Inserir novo produto          ║");
            Console.WriteLine("║  3 - Atualizar produto             ║");
            Console.WriteLine("║  4 - Deletar produto               ║");
            Console.WriteLine("║  5 - Listar por categoria          ║");
            Console.WriteLine("║                                    ║");
            Console.WriteLine("║  PEDIDOS                           ║");
            Console.WriteLine("║  6 - Criar novo pedido             ║");
            Console.WriteLine("║  7 - Listar pedidos de cliente     ║");
            Console.WriteLine("║  8 - Detalhes de um pedido         ║");
            Console.WriteLine("║                                    ║");
            Console.WriteLine("║  OUTROS                            ║");
            Console.WriteLine("║  9 - Listar categorias disponíveis ║");
            Console.WriteLine("║                                    ║");
            Console.WriteLine("║  0 - Sair                          ║");
            Console.WriteLine("╚════════════════════════════════════╝");
            Console.Write("\nEscolha uma opção: ");
        }

        // Exibe tabela de categorias
        static void ListarCategorias()
        {
            Console.WriteLine("\n=== CATEGORIAS DISPONÍVEIS ===");
            Console.WriteLine("1 - Roupas");
            Console.WriteLine("2 - Eletrônicos");
            Console.WriteLine("3 - Livros");
            Console.WriteLine("4 - Casa & Cozinha");
        }

        static void InserirNovoProduto(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== INSERIR NOVO PRODUTO ===");

            Console.Write("Nome: ");
            string nome = Console.ReadLine();

            Console.Write("Preço (ex: 199.90): ");
            decimal preco = decimal.Parse(Console.ReadLine() ?? "0");

            Console.Write("Estoque (ex: 50): ");
            int estoque = int.Parse(Console.ReadLine() ?? "0");

            Console.WriteLine("\nSelecione a categoria:");
            ListarCategorias();
            Console.Write("\nCategoriaId: ");
            int categoriaId = int.Parse(Console.ReadLine() ?? "0");

            var produto = new Produto
            {
                Nome = nome,
                Preco = preco,
                Estoque = estoque,
                CategoriaId = categoriaId
            };

            repo.InserirProduto(produto);
        }

        static void AtualizarProdutoExistente(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== ATUALIZAR PRODUTO ===");

            Console.Write("ID do produto: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            var existente = repo.BuscarPorId(id);
            if (existente == null)
            {
                Console.WriteLine("⚠️ Produto não encontrado.");
                return;
            }

            Console.WriteLine($"Atualizando: {existente.Nome} | Preço atual: {existente.Preco} | Estoque: {existente.Estoque} | CategoriaId: {existente.CategoriaId}");

            Console.Write("Novo nome (enter p/ manter): ");
            string nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome)) existente.Nome = nome;

            Console.Write("Novo preço (enter p/ manter): ");
            var precoStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(precoStr)) existente.Preco = decimal.Parse(precoStr);

            Console.Write("Novo estoque (enter p/ manter): ");
            var estStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(estStr)) existente.Estoque = int.Parse(estStr);

            Console.WriteLine("\nSelecione a nova categoria (ou enter p/ manter):");
            ListarCategorias();
            var catStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(catStr)) existente.CategoriaId = int.Parse(catStr);

            repo.AtualizarProduto(existente);
        }

        static void DeletarProdutoExistente(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== DELETAR PRODUTO ===");

            Console.Write("ID do produto: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            Console.Write("Tem certeza que deseja deletar? (s/N): ");
            var conf = Console.ReadLine();
            if (!string.Equals(conf, "s", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Operação cancelada.");
                return;
            }

            repo.DeletarProduto(id);
        }

        static void ListarPorCategoria(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== PRODUTOS POR CATEGORIA ===");
            ListarCategorias();
            Console.Write("\nInforme o CategoriaId: ");
            int categoriaId = int.Parse(Console.ReadLine() ?? "0");
            repo.ListarProdutosPorCategoria(categoriaId);
        }

        static void CriarNovoPedido(PedidoRepository repo)
        {
            Console.WriteLine("\n=== CRIAR NOVO PEDIDO ===");

            Console.Write("ClienteId: ");
            int clienteId = int.Parse(Console.ReadLine() ?? "0");

            var itens = new List<PedidoItem>();
            while (true)
            {
                Console.Write("Adicionar item? (s/N): ");
                var add = Console.ReadLine();
                if (!string.Equals(add, "s", StringComparison.OrdinalIgnoreCase)) break;

                Console.Write("ProdutoId: ");
                int produtoId = int.Parse(Console.ReadLine() ?? "0");

                Console.Write("Quantidade: ");
                int qtd = int.Parse(Console.ReadLine() ?? "0");

                Console.Write("Preço unitário (no momento do pedido): ");
                decimal precoUnit = decimal.Parse(Console.ReadLine() ?? "0");

                itens.Add(new PedidoItem
                {
                    ProdutoId = produtoId,
                    Quantidade = qtd,
                    PrecoUnitario = precoUnit
                });
            }

            var pedido = new Pedido { ClienteId = clienteId };
            repo.CriarPedido(pedido, itens);
        }

        static void ListarPedidosDeCliente(PedidoRepository repo)
        {
            Console.WriteLine("\n=== PEDIDOS DO CLIENTE ===");
            Console.Write("ClienteId: ");
            int clienteId = int.Parse(Console.ReadLine() ?? "0");
            repo.ListarPedidosCliente(clienteId);
        }

        static void DetalhesDoPedido(PedidoRepository repo)
        {
            Console.WriteLine("\n=== DETALHES DO PEDIDO ===");
            Console.Write("PedidoId: ");
            int pedidoId = int.Parse(Console.ReadLine() ?? "0");
            repo.ObterDetalhesPedido(pedidoId);
        }
    }
}
