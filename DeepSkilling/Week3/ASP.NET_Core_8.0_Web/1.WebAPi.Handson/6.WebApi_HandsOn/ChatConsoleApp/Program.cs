using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace ChatConsoleApp
{
    class Program
    {
        private const string BootstrapServers = "localhost:9092";
        private const string Topic = "console-chat-topic";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("================================================================================");
            Console.WriteLine("             APACHE KAFKA CONSOLE CHAT APPLICATION (C# .NET 8.0)                ");
            Console.WriteLine("================================================================================");

            Console.Write("Enter your Nickname to join the chat: ");
            string nickname = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname = "User_" + Guid.NewGuid().ToString().Substring(0, 4);
            }
            Console.WriteLine($"\nWelcome, {nickname}! Joining channel: '{Topic}'");
            Console.WriteLine("Connecting to Kafka broker at localhost:9092...");
            Console.WriteLine("Type your message and press Enter. Type '/exit' to quit.\n");

            // Define configuration
            var producerConfig = new ProducerConfig { BootstrapServers = BootstrapServers };
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = BootstrapServers,
                GroupId = "chat-group-" + Guid.NewGuid().ToString(), // unique group per instance
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var cts = new CancellationTokenSource();

            // Start background consumer task
            Task consumerTask = Task.Run(() => StartConsumer(consumerConfig, Topic, nickname, cts.Token));

            // Start producer loop in main thread
            await StartProducer(producerConfig, Topic, nickname, cts);

            // Wait for consumer task to exit cleanly
            try
            {
                await consumerTask;
            }
            catch (Exception) { /* Suppress cancellation exceptions */ }

            Console.WriteLine("\nThank you for using Kafka Chat App!");
        }

        private static async Task StartProducer(ProducerConfig config, string topic, string nickname, CancellationTokenSource cts)
        {
            try
            {
                using var producer = new ProducerBuilder<Null, string>(config).Build();
                while (true)
                {
                    string input = Console.ReadLine();
                    if (string.Equals(input, "/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        cts.Cancel();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(input)) continue;

                    string messagePayload = $"[{DateTime.Now:HH:mm:ss}] {nickname}: {input}";

                    try
                    {
                        // Send message asynchronously
                        var deliveryReport = await producer.ProduceAsync(topic, new Message<Null, string> { Value = messagePayload });
                        // Log delivery internally if desired
                    }
                    catch (ProduceException<Null, string> ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n[Producer Error] Failed to deliver message to Kafka: {ex.Error.Reason}");
                        Console.WriteLine("[Diagnostic Mode] Simulating message routing in local console...");
                        Console.ResetColor();
                        // Local fallback for offline simulation
                        Console.WriteLine(messagePayload);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Producer Critical Error] Could not initialize producer: {ex.Message}");
                Console.WriteLine("Please ensure Apache Kafka server is running on localhost:9092.");
                Console.ResetColor();

                // Wait for exit
                while (!cts.IsCancellationRequested)
                {
                    string input = Console.ReadLine();
                    if (string.Equals(input, "/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        cts.Cancel();
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine($"[Local-Only-Echo] [{DateTime.Now:HH:mm:ss}] {nickname}: {input}");
                    }
                }
            }
        }

        private static void StartConsumer(ConsumerConfig config, string topic, string currentNickname, CancellationToken cancellationToken)
        {
            try
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        if (consumeResult != null)
                        {
                            string message = consumeResult.Message.Value;

                            // Do not echo back our own messages if we want clean UI, or print everything.
                            // We will print everything, but highlight other users' messages in Cyan.
                            if (!message.Contains($" {currentNickname}: "))
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine(message);
                                Console.ResetColor();
                            }
                            else
                            {
                                // Print our own message with standard color to confirm topic roundtrip
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine($"{message} (delivered)");
                                Console.ResetColor();
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        // Log consume warning
                    }
                }

                consumer.Close();
            }
            catch (Exception)
            {
                // Silent catch for consumer initialization if broker is down
            }
        }
    }
}
