using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EziocraftLauncher
{
    static class Program
    {
        private const string DefaultZipUrl = "https://my.microsoftpersonalcontent.com/personal/1434fbbfed89af18/_layouts/15/download.aspx?UniqueId=5363638b-8e45-445b-8fa2-f155969bcff5&Translate=false&tempauth=v1e.eyJzaXRlaWQiOiJjY2NmNjdhNC0zYmE4LTRkZTItOTRlZi1lZGM0NDE4OWFmOWUiLCJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvbXkubWljcm9zb2Z0cGVyc29uYWxjb250ZW50LmNvbUA5MTg4MDQwZC02YzY3LTRjNWItYjExMi0zNmEzMDRiNjZkYWQiLCJleHAiOiIxNzc3NzM5MjEzIn0.rBPq5YNsgdnyL6jO5RtY_p4Bm6jAJ0cMXV0_btum208p8Z7kzX1p8alspZvTIjlCGsBIGKsDxW5i9qbb5yvZtbdGofxd4ZpLAC2fWhyIIJzg4i5HeV_YbbCJZd0jVhdckjs-DUSgaEZ4mQecw6DYj6L-4KShMqs2KNu2ELYP17vb8_jVGK7EQDXZ47Xzfz9ytrTWIpvQkjJB6K27FXNMQuo2WZXW0YiGlVDDTNkFVR8utkJLCEECorI14wwUAvuGaQjAFeZRPFXrUBdFxKOQVNBl05c1wApCCdZrm085wuyk4Js76VcOflXowEBxTbo3rtwLObA8hvI8qRPGLJ7utP979xAPA74AVj9B4UXPJNsOOnEbMNyPeTC6NyrFinCFtMUv2sTRexMjY4lMWzVZ3BA7RbxxiAbaL0qG6QXtl1blRSKS__OV4_xN1dtSN0j0rLO21AyR4sSYRc-AlWt0uBJtGbKUU0VjXQhIIo2srYMsAgkBYcxXeZGWe7p1fO_1q-Lp9gL_S9UB_yJ8w67wLQ.ICvybfZQa3qp0VkZoWFUOFI5hnVrOenX5_3qE-Jgn3o&ApiVersion=2.0";
        private static readonly string UrlConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eziocraft_url.txt");
        private static readonly string ZipUrl = LoadZipUrl();
        private const string GameName = "Eziocraft";
        private static readonly string InstallDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GameName);
        private static readonly string ZipPath = Path.Combine(InstallDir, "eziocraft.zip");
        private const string ExeName = "Eziocraft.exe";
        private static readonly PrivateFontCollection PrivateFonts = new PrivateFontCollection();
        private static readonly FontFamily LauncherFontFamily = LoadLauncherFont();
        private static readonly string LauncherFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Minecraftia-Regular.ttf");

        private static FontFamily LoadLauncherFont()
        {
            try
            {
                if (File.Exists(LauncherFontPath))
                {
                    PrivateFonts.AddFontFile(LauncherFontPath);
                    if (PrivateFonts.Families.Length > 0)
                        return PrivateFonts.Families[0];
                }
            }
            catch
            {
            }

            return SystemFonts.DefaultFont.FontFamily;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public class MainForm : Form
        {
            private readonly Label statusLabel;
            private readonly Button installButton;
            private readonly Button launchButton;
            private readonly Button openFolderButton;
            private readonly Button editConfigButton;

            public MainForm()
            {
                Text = GameName + " Launcher";
                Width = 480;
                Height = 240;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                StartPosition = FormStartPosition.CenterScreen;
                Font = new System.Drawing.Font(LauncherFontFamily, 9f);

                var installLabel = new Label
                {
                    Text = "Dossier d'installation : " + InstallDir,
                    AutoSize = false,
                    Width = 440,
                    Height = 40,
                    Left = 16,
                    Top = 16
                };
                Controls.Add(installLabel);

                statusLabel = new Label
                {
                    Text = "Prêt",
                    AutoSize = false,
                    Width = 440,
                    Height = 24,
                    Left = 16,
                    Top = 64
                };
                Controls.Add(statusLabel);

                installButton = new Button
                {
                    Text = "Installer / Mettre à jour",
                    Width = 200,
                    Height = 32,
                    Left = 16,
                    Top = 104
                };
                installButton.Click += async (sender, args) => await InstallOrUpdateAsync();
                Controls.Add(installButton);

                launchButton = new Button
                {
                    Text = "Lancer le jeu",
                    Width = 200,
                    Height = 32,
                    Left = 240,
                    Top = 104
                };
                launchButton.Click += (sender, args) => LaunchGame();
                Controls.Add(launchButton);

                openFolderButton = new Button
                {
                    Text = "Ouvrir le dossier",
                    Width = 200,
                    Height = 32,
                    Left = 16,
                    Top = 152
                };
                openFolderButton.Click += (sender, args) => OpenInstallDir();
                Controls.Add(openFolderButton);

                editConfigButton = new Button
                {
                    Text = "Modifier le zip URL",
                    Width = 200,
                    Height = 32,
                    Left = 240,
                    Top = 152
                };
                editConfigButton.Click += (sender, args) => EditConfig();
                Controls.Add(editConfigButton);
            }

            private void SetStatus(string text)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => SetStatus(text)));
                    return;
                }
                statusLabel.Text = text;
            }

            private async Task InstallOrUpdateAsync()
            {
                installButton.Enabled = false;
                SetStatus("Téléchargement du zip...");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                try
                {
                    if (ZipUrl.Contains("example.com"))
                        throw new InvalidOperationException("URL du ZIP invalide. Modifiez la constante ZipUrl dans le code pour utiliser le fichier ZIP réel du jeu.");

                    Directory.CreateDirectory(InstallDir);
                    await DownloadZipAsync(ZipUrl, ZipPath);
                    SetStatus("Extraction du zip...");
                    ExtractZip(ZipPath, InstallDir);
                    SetStatus("Installation terminée.");
                    MessageBox.Show("Installation / mise à jour terminée.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    SetStatus("Erreur pendant l'installation.");
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    installButton.Enabled = true;
                }
            }

            private static async Task DownloadZipAsync(string url, string destFile)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                const int maxAttempts = 3;
                using (var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) })
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    for (int attempt = 1; attempt <= maxAttempts; attempt++)
                    {
                        try
                        {
                            if (File.Exists(destFile))
                            {
                                File.Delete(destFile);
                            }

                            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    var statusMessage = "Échec du téléchargement (" + (int)response.StatusCode + ") : " + response.ReasonPhrase + ".";
                                    if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        statusMessage += " Le lien ZIP semble privé ou expiré. Mettez à jour eziocraft_url.txt avec un lien public direct.";
                                    }
                                    throw new InvalidOperationException(statusMessage);
                                }

                                using (var stream = await response.Content.ReadAsStreamAsync())
                                using (var fileStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
                                {
                                    await stream.CopyToAsync(fileStream);
                                }
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            if (attempt < maxAttempts && IsTransientNetworkError(ex))
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(2 * attempt));
                                continue;
                            }
                            throw new InvalidOperationException("Échec du téléchargement : " + ex.Message, ex);
                        }
                    }
                }
            }

            private static bool IsTransientNetworkError(Exception ex)
            {
                return ex is HttpRequestException || ex is IOException || ex is TaskCanceledException;
            }

            private static void ExtractZip(string zipPath, string targetDir)
            {
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var destinationPath = Path.GetFullPath(Path.Combine(targetDir, entry.FullName));
                        if (!destinationPath.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase))
                            throw new IOException("Chemin d'accès ZIP non valide.");

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        entry.ExtractToFile(destinationPath, true);
                    }
                }
            }

            private void LaunchGame()
            {
                try
                {
                    var exePath = FindGameExecutable(InstallDir, ExeName);
                    if (exePath == null)
                    {
                        MessageBox.Show("Impossible de trouver l'exécutable du jeu. Installez d'abord le jeu.", "Jeu introuvable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var workingDir = Path.GetDirectoryName(exePath) ?? InstallDir;
                    Process.Start(new ProcessStartInfo(exePath)
                    {
                        WorkingDirectory = workingDir,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de lancer le jeu : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private static string FindGameExecutable(string rootDir, string exeName)
            {
                if (!Directory.Exists(rootDir))
                    return null;

                var exactMatch = Directory.GetFiles(rootDir, exeName, SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(exactMatch))
                    return exactMatch;

                var firstExe = Directory.GetFiles(rootDir, "*.exe", SearchOption.AllDirectories)
                    .FirstOrDefault(path => !path.EndsWith("EziocraftLauncher.exe", StringComparison.OrdinalIgnoreCase));
                return firstExe;
            }

            private void OpenInstallDir()
            {
                if (Directory.Exists(InstallDir))
                {
                    Process.Start(new ProcessStartInfo(InstallDir) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Le dossier d'installation n'existe pas encore.", "Dossier introuvable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            private void EditConfig()
            {
                try
                {
                    if (!File.Exists(UrlConfigPath))
                    {
                        File.WriteAllText(UrlConfigPath, DefaultZipUrl);
                    }

                    Process.Start(new ProcessStartInfo("notepad.exe", UrlConfigPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible d'ouvrir le fichier de configuration : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string LoadZipUrl()
        {
            try
            {
                if (File.Exists(UrlConfigPath))
                {
                    var url = File.ReadAllText(UrlConfigPath).Trim();
                    if (!string.IsNullOrEmpty(url))
                        return url;
                }
                else
                {
                    File.WriteAllText(UrlConfigPath, DefaultZipUrl);
                }
            }
            catch
            {
            }

            return DefaultZipUrl;
        }
    }
}
