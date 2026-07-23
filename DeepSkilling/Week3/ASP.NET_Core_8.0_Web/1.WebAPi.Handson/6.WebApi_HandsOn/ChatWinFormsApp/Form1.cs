using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Confluent.Kafka;

namespace ChatWinFormsApp
{
    public partial class Form1 : Form
    {
        // Kafka variables
        private IProducer<Null, string>? _producer;
        private CancellationTokenSource? _cts;
        private const string Topic = "winforms-chat-topic";
        private bool _isConnected = false;

        // UI Controls
        private Panel pnlConnect = null!;
        private Label lblNickname = null!;
        private TextBox txtNickname = null!;
        private Label lblBroker = null!;
        private TextBox txtBroker = null!;
        private Button btnConnect = null!;

        private RichTextBox rtbMessages = null!;
        private Panel pnlInput = null!;
        private TextBox txtMessage = null!;
        private Button btnSend = null!;

        public Form1()
        {
            InitializeComponent();
            SetupCustomLayout();
        }

        private void SetupCustomLayout()
        {
            // Set Form Properties
            this.Text = "Apache Kafka Multi-Client Chat Application";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(33, 33, 33);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // Connect Panel
            pnlConnect = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10)
            };

            lblNickname = new Label
            {
                Text = "Nickname:",
                Location = new Point(10, 20),
                AutoSize = true,
                ForeColor = Color.LightGray
            };

            txtNickname = new TextBox
            {
                Text = "Client_" + Guid.NewGuid().ToString().Substring(0, 4),
                Location = new Point(90, 17),
                Width = 120,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblBroker = new Label
            {
                Text = "Kafka Broker:",
                Location = new Point(230, 20),
                AutoSize = true,
                ForeColor = Color.LightGray
            };

            txtBroker = new TextBox
            {
                Text = "localhost:9092",
                Location = new Point(330, 17),
                Width = 140,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnConnect = new Button
            {
                Text = "Connect",
                Location = new Point(490, 14),
                Width = 100,
                Height = 32,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;

            pnlConnect.Controls.AddRange(new Control[] { lblNickname, txtNickname, lblBroker, txtBroker, btnConnect });

            // Messages Textbox
            rtbMessages = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // Input Panel
            pnlInput = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10)
            };

            txtMessage = new TextBox
            {
                Location = new Point(10, 15),
                Width = 470,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtMessage.KeyDown += TxtMessage_KeyDown;

            btnSend = new Button
            {
                Text = "Send Message",
                Location = new Point(490, 12),
                Width = 180,
                Height = 32,
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            pnlInput.Controls.AddRange(new Control[] { txtMessage, btnSend });

            // Add all controls to Form
            this.Controls.Add(rtbMessages);
            this.Controls.Add(pnlConnect);
            this.Controls.Add(pnlInput);

            AppendLog("SYSTEM", "Welcome to Kafka Chat Application! Set nickname and connect to localhost:9092 to start.");
        }

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (!_isConnected)
            {
                string broker = txtBroker.Text.Trim();
                string nickname = txtNickname.Text.Trim();

                if (string.IsNullOrEmpty(broker) || string.IsNullOrEmpty(nickname))
                {
                    MessageBox.Show("Please specify both Nickname and Broker Address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendLog("SYSTEM", $"Connecting to Kafka Broker at {broker}...");

                var producerConfig = new ProducerConfig
                {
                    BootstrapServers = broker,
                    MessageSendMaxRetries = 1,
                    SocketTimeoutMs = 3000
                };

                try
                {
                    _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
                    _isConnected = true;
                    btnConnect.Text = "Disconnect";
                    btnConnect.BackColor = Color.FromArgb(204, 50, 50);
                    btnSend.Enabled = true;
                    txtNickname.ReadOnly = true;
                    txtBroker.ReadOnly = true;

                    // Start background consumer
                    _cts = new CancellationTokenSource();
                    string groupId = "winforms-group-" + Guid.NewGuid().ToString().Substring(0, 8);
                    var consumerConfig = new ConsumerConfig
                    {
                        BootstrapServers = broker,
                        GroupId = groupId,
                        AutoOffsetReset = AutoOffsetReset.Earliest
                    };

                    Task.Run(() => StartKafkaConsumer(consumerConfig, Topic, nickname, _cts.Token));

                    AppendLog("SYSTEM", $"Successfully connected to Kafka broker! Chat channel: '{Topic}' active.");
                }
                catch (Exception ex)
                {
                    AppendLog("SYSTEM-ERROR", $"Connection failed: {ex.Message}");
                    MessageBox.Show($"Could not connect to Kafka Broker: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            _isConnected = false;
            _cts?.Cancel();
            _producer?.Dispose();
            _producer = null;

            btnConnect.Text = "Connect";
            btnConnect.BackColor = Color.FromArgb(0, 122, 204);
            btnSend.Enabled = false;
            txtNickname.ReadOnly = false;
            txtBroker.ReadOnly = false;

            AppendLog("SYSTEM", "Disconnected from Kafka Broker.");
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void TxtMessage_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // suppress default Windows beep
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (!_isConnected || _producer == null) return;

            string msgText = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msgText)) return;

            string nickname = txtNickname.Text.Trim();
            string messagePayload = $"[{DateTime.Now:HH:mm:ss}] {nickname}: {msgText}";

            txtMessage.Clear();

            try
            {
                var result = await _producer.ProduceAsync(Topic, new Message<Null, string> { Value = messagePayload });
            }
            catch (Exception ex)
            {
                AppendLog("SYSTEM-ERROR", $"Send failed: {ex.Message}");
            }
        }

        private void StartKafkaConsumer(ConsumerConfig config, string topic, string currentNickname, CancellationToken token)
        {
            try
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(topic);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(token);
                        if (result != null)
                        {
                            string msg = result.Message.Value;

                            // Update UI thread safely
                            this.BeginInvoke(new Action(() =>
                            {
                                if (msg.Contains($" {currentNickname}: "))
                                {
                                    AppendLog(currentNickname, msg.Substring(msg.IndexOf(currentNickname) + currentNickname.Length + 2), isSelf: true);
                                }
                                else
                                {
                                    // Parse nickname
                                    int startIdx = msg.IndexOf(']') + 2;
                                    int endIdx = msg.IndexOf(':', startIdx);
                                    if (endIdx > startIdx)
                                    {
                                        string senderNick = msg.Substring(startIdx, endIdx - startIdx);
                                        string body = msg.Substring(endIdx + 2);
                                        AppendLog(senderNick, body, isSelf: false);
                                    }
                                    else
                                    {
                                        AppendLog("Unknown", msg, isSelf: false);
                                    }
                                }
                            }));
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            AppendLog("SYSTEM-WARNING", $"Consume warning: {ex.Error.Reason}");
                        }));
                    }
                    catch (Exception)
                    {
                        // exit loop if cancelled/errored
                        break;
                    }
                }

                consumer.Close();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    AppendLog("SYSTEM-ERROR", $"Consumer initialization failed: {ex.Message}");
                }));
            }
        }

        private void AppendLog(string sender, string message, bool isSelf = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            rtbMessages.SelectionStart = rtbMessages.TextLength;
            rtbMessages.SelectionLength = 0;

            if (sender.StartsWith("SYSTEM"))
            {
                rtbMessages.SelectionColor = Color.Yellow;
                rtbMessages.AppendText($"[{timestamp}] {message}\n");
            }
            else if (sender.EndsWith("ERROR"))
            {
                rtbMessages.SelectionColor = Color.LightCoral;
                rtbMessages.AppendText($"[{timestamp}] {message}\n");
            }
            else
            {
                rtbMessages.SelectionColor = Color.Gray;
                rtbMessages.AppendText($"[{timestamp}] ");

                if (isSelf)
                {
                    rtbMessages.SelectionColor = Color.LightGreen;
                    rtbMessages.AppendText($"{sender} (You): ");
                }
                else
                {
                    rtbMessages.SelectionColor = Color.DeepSkyBlue;
                    rtbMessages.AppendText($"{sender}: ");
                }

                rtbMessages.SelectionColor = Color.White;
                rtbMessages.AppendText($"{message}\n");
            }

            rtbMessages.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Disconnect();
            base.OnFormClosing(e);
        }
    }
}
