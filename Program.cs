#region Version 1
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;






namespace ConsoleApp1
{

    #region Lectura PDF

    public enum Meses
    {
        ENE, FEB, MAR, ABR, MAY, JUN, JUL, AGO, SEP, OCT, NOV, DIC
    }
    public enum TipoDireccion
    {
        RES, LAB, CRR
    }

    public class InformacionBasica
    {
        public string Nombre { get; set; }
        public string CC { get; set; }
        public string EstadoDocumento { get; set; }
        public string RangoEdad { get; set; }
        public string LugarExpedicion { get; set; }
        public string FechaExpedicion { get; set; }
        public string Genero { get; set; }
        public string Ciiu { get; set; }
        public string ActividadEconomica { get; set; }
    }

    public class Direcciones_Registradas
    {
        public string Direccion { get; set; }
        public string Ciudad { get; set; }

    }

    public class TelefonoFijo
    {
        public int NumeroOrden { get; set; }
        public string Telefono { get; set; }
        public string Tipo { get; set; }
        public string Ciudad { get; set; }
        public string Departamento { get; set; }
        public string ReportadoDesde { get; set; }
        public string UltimoReporte { get; set; }
        public int NumeroReportes { get; set; }
        public int NumeroEntidades { get; set; }
        public string Fuente { get; set; }
    }

    public class TelefonoCelular
    {
        public int NumeroOrden { get; set; }
        public string Telefono { get; set; }
        public string ReportadoDesde { get; set; }
        public string UltimoReporte { get; set; } 
        public int NumeroReportes { get; set; }
        public int NumeroEntidades { get; set; } 
        public string Fuente { get; set; }
    }

    public class CorreoElectronico
    {
        public int NumeroOrden { get; set; }
        public string Correo { get; set; }
        public string ReportadoDesde { get; set; }
        public string UltimoReporte { get; set; }
        public int NumeroReportes { get; set; }
        public string Fuente { get; set; }
    }

    public class PdfFolderInfo
    {
        public string FolderName { get; set; }
        public string PdfFilePath { get; set; } 
    }

    public class InformeDataCredito
    {

        public InformacionBasica InformacionBasica { get; set; }
        public List<Direcciones_Registradas> Direcciones { get; set; }
        public List<TelefonoFijo> TelefonosFijos { get; set; }
        public List<TelefonoCelular> TelefonosCelulares { get; set; }
        public List<CorreoElectronico> CorreosElectronicos { get; set; }
        public string FolderName { get; set; }
    }

    #endregion 
    class Program
    {
        private static List<InformeDataCredito> informesGlobales = new List<InformeDataCredito>();
        private static string directorioInicial = "";
        static void Main()
        {
            Console.WriteLine("Procedimiento a realizar :\n1. Leer PDF\n2. Renombrar Archivos\n3. Salir");

            string input = Console.ReadLine();
            int seleccion;

            if (int.TryParse(input, out seleccion))
            {
                switch (seleccion)
                {
                    case 1:
                        Console.WriteLine("Por favor, ingrese la ruta del directorio:");
                        string inputPath = Console.ReadLine();

                        // Validar y establecer la ruta del directorio inicial
                        directorioInicial = ValidarDirectorio(inputPath);

                        // Verificar si el directorio existe
                        if (directorioInicial != "")
                        {
                            // Leer todas las carpetas en el directorio
                            string[] directories = Directory.GetDirectories(directorioInicial);

                            foreach (string directory in directories)
                            {
                                string folderName = Path.GetFileName(directory);
                                string[] pdfFiles = Directory.GetFiles(directory, "*.pdf");

                                foreach (string pdfFile in pdfFiles)
                                {
                                    ProcessPdfFile(pdfFile, folderName);
                                }
                            }

                            // Después de procesar todos los archivos, guardar los informes como JSON
                            GuardarInformesComoJson();
                        }
                        else
                        {
                            Console.WriteLine("El directorio no existe o no es válido.");
                        }

                        Console.WriteLine("Presione cualquier tecla para salir...");
                        Console.ReadKey();

                        break;
                    case 2:
                        Console.WriteLine("Renombrar Archivos seleccionado.");
                        // Lógica para renombrar archivos aquí
                        break;
                    case 3:
                        Console.WriteLine("Salir seleccionado.");
                        // Lógica para salir aquí
                        break;
                    default:
                        Console.WriteLine("Selección no válida. Por favor, elija 1, 2 o 3.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Entrada no válida. Por favor, introduzca un número entero.");
            }

           

        }

        static string ValidarDirectorio(string path)
        {
            try
            {
                // Validar si el directorio existe
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    Console.WriteLine("El directorio especificado no existe.");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar el directorio: {ex.Message}");
                return "";
            }
        }

        static void ProcessPdfFile(string pdfFilePath, string folderName)
        {
            try
            {
                var pdfDocument = new PdfDocument(new PdfReader(pdfFilePath));
                var strategy = new LocationTextExtractionStrategy();
                string text = string.Empty;

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    text += PdfTextExtractor.GetTextFromPage(page, strategy);
                }

                var informe = ParsePdfTextToInforme(text);
                informe.FolderName = folderName;
                informesGlobales.Add(informe);

                Console.WriteLine($"\nInforme del archivo {pdfFilePath} Guardado!");

                // Renombrar el archivo PDF usando el número de documento (CC)
                if (!string.IsNullOrWhiteSpace(informe.InformacionBasica.CC))
                {
                    string newFilePath = Path.Combine(Path.GetDirectoryName(pdfFilePath), informe.InformacionBasica.CC + ".pdf");
                    File.Move(pdfFilePath, newFilePath);
                    Console.WriteLine($"Archivo renombrado a: {newFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando el archivo {pdfFilePath}: {ex.Message}");
            }
        }


        static void GuardarInformesComoJson()
        {
            string jsonFilePath = Path.Combine(directorioInicial, "informes.json");

            try
            {
                // Serializar la lista informesGlobales a formato JSON
                string json = JsonConvert.SerializeObject(informesGlobales, Formatting.Indented);

                // Escribir el JSON en un archivo
                File.WriteAllText(jsonFilePath, json);

                Console.WriteLine($"Archivo JSON guardado exitosamente en: {jsonFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el archivo JSON: {ex.Message}");
            }
        }


        #region Codigo para converir PDF a texto

        #endregion

        #region <Metodo Lectura PDF>
        static InformeDataCredito ParsePdfTextToInforme(string text)
        {
            var informe = new InformeDataCredito
            {
                InformacionBasica = new InformacionBasica(),
                Direcciones = new List<Direcciones_Registradas>(),
                TelefonosFijos = new List<TelefonoFijo>(),
                TelefonosCelulares = new List<TelefonoCelular>(),
                CorreosElectronicos = new List<CorreoElectronico>()
            };

            // Parsing lógica para cada sección
            ParseInformacionBasica(text, informe.InformacionBasica);
            ParseDirecciones(text, informe.Direcciones);
            //ParseVectorDeDirecciones(text, informe.VectorDeDirecciones);
            ParseTelefonosFijos(text, informe.TelefonosFijos);
            ParseTelefonosCelulares(text, informe.TelefonosCelulares);
            ParseCorreosElectronicos(text, informe.CorreosElectronicos);

            return informe;
        }

        static void ParseInformacionBasica(string text, InformacionBasica infoBasica)
        {
            // Buscar el encabezado "INFORMACIÓN BÁSICA"
            var basicInfoStartIndex = text.IndexOf("INFORMACIÓN BÁSICA");
            if (basicInfoStartIndex == -1) return;

            // Extraer la sección de información básica
            var basicInfoSection = text.Substring(basicInfoStartIndex);

            // Extraer el nombre hasta " - C.C."
            var nombreMatch = Regex.Match(basicInfoSection, @"INFORMACIÓN BÁSICA\s*(.*?)\s*- C.C. (\d+)");
            if (nombreMatch.Success)
            {
                infoBasica.Nombre = nombreMatch.Groups[1].Value.Trim();
                infoBasica.CC = nombreMatch.Groups[2].Value.Trim();
            }

            // Extraer los otros campos
            var estadoMatch = Regex.Match(basicInfoSection, @"Estado Documento\s*(\w+)");
            if (estadoMatch.Success)
            {
                infoBasica.EstadoDocumento = estadoMatch.Groups[1].Value.Trim();
            }

            var rangoEdadMatch = Regex.Match(basicInfoSection, @"Rango Edad\s*(\d+-\d+)");
            if (rangoEdadMatch.Success)
            {
                infoBasica.RangoEdad = rangoEdadMatch.Groups[1].Value.Trim();
            }

            var lugarExpedicionMatch = Regex.Match(basicInfoSection, @"Lugar Expedición\s*(.*?)\s");
            if (lugarExpedicionMatch.Success)
            {
                infoBasica.LugarExpedicion = lugarExpedicionMatch.Groups[1].Value.Trim();
            }

            var fechaExpedicionMatch = Regex.Match(basicInfoSection, @"Fecha Expedición\s*(\d{2}-\w{3}-\d{2})");
            if (fechaExpedicionMatch.Success)
            {
                infoBasica.FechaExpedicion = fechaExpedicionMatch.Groups[1].Value.Trim();
            }

            var generoMatch = Regex.Match(basicInfoSection, @"Género\s*(\w+)");
            if (generoMatch.Success)
            {
                infoBasica.Genero = generoMatch.Groups[1].Value.Trim();
            }

            var ciiuMatch = Regex.Match(basicInfoSection, @"CIIU\s*(\d+)");
            if (ciiuMatch.Success)
            {
                infoBasica.Ciiu = ciiuMatch.Groups[1].Value.Trim();
            }

            var actividadEconomicaMatch = Regex.Match(basicInfoSection, @"Actividad Económica\s*(.*?)\s*(Estado|Rango Edad|Lugar Expedición|Fecha Expedición|Género|CIIU)");
            if (actividadEconomicaMatch.Success)
            {
                infoBasica.ActividadEconomica = actividadEconomicaMatch.Groups[1].Value.Trim();
            }
        }

        static void ParseDirecciones(string text, List<Direcciones_Registradas> direcciones)
        {
            string[] lines = text.Split('\n');
            bool inDireccionesSection = false;

            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("DIRECCIONES"))
                {
                    inDireccionesSection = true;
                    continue;
                }

                if (inDireccionesSection && !string.IsNullOrWhiteSpace(line))
                {
                    if (line.Contains("URB")) // Validación adicional
                    {
                        string direccion = "";
                        string ciudad = "";

                        int indexURB = line.IndexOf("URB");
                        if (indexURB != -1)
                        {
                            direccion = line.Substring(1, indexURB - 1).Trim();

                            int startIndexCiudad = indexURB + 3; // Se suma 3 para omitir "URB"
                            int lengthCiudad = Math.Min(80, line.Length - startIndexCiudad);
                            ciudad = line.Substring(startIndexCiudad, lengthCiudad).Trim();

                            // Eliminar caracteres innecesarios de la ciudad
                            foreach (Meses mes in Enum.GetValues(typeof(Meses)))
                            {
                                int indexMes = ciudad.IndexOf(mes.ToString());
                                if (indexMes != -1)
                                {
                                    ciudad = ciudad.Substring(0, indexMes); // Eliminar desde el punto de coincidencia hasta el final del string
                                    break;
                                }
                            }

                            // Eliminar caracteres desde donde inicie la coincidencia de "- RES", "- LAB" o "- CRR"
                            EliminarCaracteresDesdeInicioCoincidencia(ref direccion, "- RES", "- LAB", "- CRR");

                            if (!string.IsNullOrWhiteSpace(ciudad))
                            {
                                EliminarDesdeCoincidenciaMeses(ref ciudad);

                                if (!string.IsNullOrWhiteSpace(direccion))
                                {
                                    Direcciones_Registradas direccionRegistrada = new Direcciones_Registradas
                                    {
                                        Direccion = direccion,
                                        Ciudad = ciudad
                                    };
                                    direcciones.Add(direccionRegistrada);
                                }
                            }
                        }
                    }
                }
            }
        }

        static void EliminarDesdeCoincidenciaMeses(ref string text)
        {
            foreach (Meses mes in Enum.GetValues(typeof(Meses)))
            {
                int indexMes = text.IndexOf(mes.ToString());
                if (indexMes != -1)
                {
                    text = text.Substring(0, indexMes); // Eliminar desde el punto de coincidencia hasta el final del string
                    break;
                }
            }
        }
        static void EliminarCaracteresDesdeInicioCoincidencia(ref string text, params string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                int index = text.IndexOf(pattern);
                if (index != -1)
                {
                    text = text.Substring(0, index); // Eliminar desde donde inicie la coincidencia hasta el final del string
                    break;
                }
            }
        }

        static void ParseTelefonosFijos(string text, List<TelefonoFijo> telefonosFijos)
        {
            var telefonoFijoMatches = Regex.Matches(text, @"(\d+)\s+(\d+)\s+([A-Z]+)\s+([A-Z\s]+)\s+([A-Z\s]+)\s+([A-Z]{3}\s+-\s+\d{4})\s+([A-Z]{3}\s+-\s+\d{4})\s+(\d+)\s+(\d+)\s+([A-Z]+)");
            foreach (Match match in telefonoFijoMatches)
            {
                if (match.Success)
                {
                    var telefonoFijo = new TelefonoFijo
                    {
                        NumeroOrden = int.Parse(match.Groups[1].Value),
                        Telefono = match.Groups[2].Value,
                        Tipo = match.Groups[3].Value,
                        Ciudad = match.Groups[4].Value,
                        Departamento = match.Groups[5].Value,
                        ReportadoDesde = match.Groups[6].Value,
                        UltimoReporte = match.Groups[7].Value,
                        NumeroReportes = int.Parse(match.Groups[8].Value),
                        NumeroEntidades = int.Parse(match.Groups[9].Value),
                        Fuente = match.Groups[10].Value
                    };
                    telefonosFijos.Add(telefonoFijo);
                }
            }
        }

        static void ParseTelefonosCelulares(string text, List<TelefonoCelular> telefonosCelulares)
        {
            var telefonoCelularMatches = Regex.Matches(text, @"(\d+)\s+(\d+)\s+([A-Z]{3}\s+-\s+\d{4})\s+([A-Z]{3}\s+-\s+\d{4})\s+(\d+)\s+(\d+)\s+([A-Z]+)");
            foreach (Match match in telefonoCelularMatches)
            {
                if (match.Success)
                {
                    var telefonoCelular = new TelefonoCelular
                    {
                        NumeroOrden = int.Parse(match.Groups[1].Value),
                        Telefono = match.Groups[2].Value,
                        ReportadoDesde = match.Groups[3].Value,
                        UltimoReporte = match.Groups[4].Value,
                        NumeroReportes = int.Parse(match.Groups[5].Value),
                        NumeroEntidades = int.Parse(match.Groups[6].Value),
                        Fuente = match.Groups[7].Value
                    };
                    telefonosCelulares.Add(telefonoCelular);
                }
            }
        }

        static void ParseCorreosElectronicos(string text, List<CorreoElectronico> correosElectronicos)
        {
            var correoElectronicoMatches = Regex.Matches(text, @"(\d+)\s+([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})\s+([A-Z]{3}\s+-\s+\d{4})\s+([A-Z]{3}\s+-\s+\d{4})\s+(\d+)\s+([A-Z]+)");
            foreach (Match match in correoElectronicoMatches)
            {
                if (match.Success)
                {
                    var correoElectronico = new CorreoElectronico
                    {
                        NumeroOrden = int.Parse(match.Groups[1].Value),
                        Correo = match.Groups[2].Value,
                        ReportadoDesde = match.Groups[3].Value,
                        UltimoReporte = match.Groups[4].Value,
                        NumeroReportes = int.Parse(match.Groups[5].Value),
                        Fuente = match.Groups[6].Value
                    };
                    correosElectronicos.Add(correoElectronico);
                }
            }

        }

        #endregion

    }
}
  
#endregion