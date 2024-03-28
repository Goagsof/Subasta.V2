using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Subasta.Models;
using System.Data.SqlTypes;
using System.Threading;
using System.Net.Mail;
using System.Net;

namespace Subasta.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString;

        public HomeController()
        {
            LibroV libroV = new LibroV();
            connectionString = libroV.connectionString;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            string procedimientoAlmacenado = "ValidarCredenciales";
            bool credencialesValidas = false;
            bool bloqueado = false;
            bool error = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", model.CorreoElectronico);
                    command.Parameters.AddWithValue("@Contraseña", model.Contraseña);

                    connection.Open();

                    string resultado = (string)command.ExecuteScalar();

                    if (resultado == "OK")
                    {
                        credencialesValidas = true;
                        Session["CorreoElectronico"] = model.CorreoElectronico;
                    }
                    else if (resultado == "Bloqueado")
                    {
                        bloqueado = true;
                    }
                    else if (resultado == "Error")
                    {
                        error = true;
                    }

                    connection.Close();

                    RegistrarIntentoLogin(model.CorreoElectronico, resultado);
                }
            }

            if (credencialesValidas)
            {
                Session["UserID"] = ObtenerUserIDPorCorreoElectronico(model.CorreoElectronico);
                return RedirectToAction("Doble");
            }
            else if (bloqueado)
            {
                ViewBag.Error = "Tu cuenta ha sido bloqueada debido a múltiples intentos.";
                return View(model);
            }
            else if (error)
            {
                ViewBag.Error = "Correo electrónico o contraseña incorrectos";
                return View(model);
            }
            else
            {
                ViewBag.Error = "Correo electrónico o contraseña incorrectos";
                return View(model);
            }
        }


        private void RegistrarIntentoLogin(string correoElectronico, string resultado)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO LoginAudit (CorreoElectronico, FechaHoraIntento, Exito, MotivoBloqueo) VALUES (@CorreoElectronico, @FechaHoraIntento, @Exito, @MotivoBloqueo)";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@CorreoElectronico", correoElectronico);
                command.Parameters.AddWithValue("@FechaHoraIntento", DateTime.Now);
                command.Parameters.AddWithValue("@Exito", resultado == "OK");
                command.Parameters.AddWithValue("@MotivoBloqueo", resultado == "Bloqueado" ? "Múltiples intentos fallidos" : "");

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al registrar el intento de inicio de sesión en la auditoría: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }


        private int ObtenerUserIDPorCorreoElectronico(string correoElectronico)
        {
            int userID = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string procedimientoAlmacenado = "ObtenerUserIDPorCorreoElectronico";

                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", correoElectronico);

                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        userID = Convert.ToInt32(result);
                    }
                }
            }

            return userID;
        }

        public ActionResult Doble()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerificarCodigo(string codigo)
        {
            string correoElectronico = Session["CorreoElectronico"] as string;

            string resultado = ConsultarCodigo(correoElectronico, codigo);

            if (resultado == "OK")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "El código ingresado no es válido.";
                return View("Doble");
            }
        }

        private string ConsultarCodigo(string correoElectronico, string codigo)
        {
            string resultado = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string procedimientoAlmacenado = "ConsultarCodigo";

                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", correoElectronico);
                    command.Parameters.AddWithValue("@Codigo", codigo);
                    SqlParameter outputParameter = new SqlParameter("@Resultado", SqlDbType.VarChar, 10);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    connection.Open();
                    command.ExecuteNonQuery();

                    resultado = command.Parameters["@Resultado"].Value.ToString();
                }
            }

            return resultado;
        }

        private string ConsultarCodigoYAlmacenar(string correoElectronico)
        {
            string codigo = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string procedimientoAlmacenado = "ConsultarCodigoYAlmacenar";

                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", correoElectronico);
                    SqlParameter outputParameter = new SqlParameter("@Codigo", SqlDbType.VarChar, 6);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    connection.Open();
                    command.ExecuteNonQuery();

                    codigo = command.Parameters["@Codigo"].Value.ToString();

                    EnviarCorreoConCodigo(correoElectronico, codigo);
                }
            }

            return codigo;
        }


        public void EnviarCorreoConCodigo(string destinatario, string codigo)
        {
            try
            {
                string remitente = "gagohm@gmail.com"; 
                string clave = "pbzn cagt eqyg dngb"; 

                MailMessage mensaje = new MailMessage(remitente, destinatario);
                mensaje.Subject = "Código de verificación";
                mensaje.Body = $"Tu código de verificación es: {codigo}";

                SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com", 587);
                clienteSmtp.EnableSsl = true;
                clienteSmtp.Credentials = new NetworkCredential(remitente, clave);

                clienteSmtp.Send(mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo electrónico: " + ex.Message);
            }
        }

        public ActionResult EnviarCodigo()
        {
            string correoElectronico = Session["CorreoElectronico"] as string;

            string codigo = ConsultarCodigoYAlmacenar(correoElectronico);

            ViewBag.Message = "Código enviado correctamente.";

            return View("Doble");
        }


        public ActionResult Index()
        {
            return ObtenerSubastasDesdeBD();
        }

        public ActionResult ObtenerSubastasDesdeBD()
        {
            var subastas = new List<InicioViewModel>();

            LibroV libroV = new LibroV();
            string connectionString = libroV.connectionString;

            string query = @"
        UPDATE Subastas
        SET Estado = 'Finalizado'
        WHERE FechaFin <= GETDATE() AND Estado = 'Activa';

        SELECT * FROM Subastas WHERE Estado = 'Activa';
    ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        subastas.Add(new InicioViewModel
                        {
                            SubastaID = Convert.ToInt32(reader["SubastaID"]),
                            Titulo = reader["Titulo"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            PrecioActual = Convert.ToDecimal(reader["PrecioActual"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Estado = reader["Estado"].ToString(),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            ImagenProducto = reader["ImagenProducto"].ToString()
                        });
                    }

                    reader.Close();
                }
            }
            return View(subastas);
        }



        public ActionResult Registro()
        {
            var preguntasModel = new PreguntaViewModel();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerPreguntasSeguridad";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        preguntasModel.Preguntas = new List<PreguntaSeguridad>();
                        while (reader.Read())
                        {
                            PreguntaSeguridad pregunta = new PreguntaSeguridad
                            {
                                PreguntaID = Convert.ToInt32(reader["PreguntaID"]),
                                Pregunta = reader["Pregunta"].ToString()
                            };
                            preguntasModel.Preguntas.Add(pregunta);
                        }
                        reader.Close();
                    }
                }

                var registroModel = new RegistroViewModel
                {
                    PreguntasDeSeguridad = preguntasModel.Preguntas
                };

                return View(registroModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar el formulario de registro: " + ex.Message;
                return View(new RegistroViewModel());
            }
        }

        [HttpPost]
        public ActionResult Registro(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Contraseña != model.ConfirmarContraseña)
                {
                    ModelState.AddModelError("ConfirmarContraseña", "Las contraseñas no coinciden");
                    return View(model);
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "RegistrarUsuario";

                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Nombre", model.Nombre);
                        command.Parameters.AddWithValue("@Apellido", model.Apellido);
                        command.Parameters.AddWithValue("@CorreoElectronico", model.CorreoElectronico);
                        command.Parameters.AddWithValue("@Contraseña", model.Contraseña);
                        command.Parameters.AddWithValue("@PreguntaID1", model.PreguntaID1);
                        command.Parameters.AddWithValue("@Respuesta1", model.Respuesta1);
                        command.Parameters.AddWithValue("@PreguntaID2", model.PreguntaID2);
                        command.Parameters.AddWithValue("@Respuesta2", model.Respuesta2);
                        command.Parameters.AddWithValue("@PreguntaID3", model.PreguntaID3);
                        command.Parameters.AddWithValue("@Respuesta3", model.Respuesta3);
                        command.Parameters.AddWithValue("@Provincia", model.Provincia);
                        command.Parameters.AddWithValue("@Canton", model.Canton);
                        command.Parameters.AddWithValue("@Distrito", model.Distrito);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            return RedirectToAction("Login", "Home");
                        }
                    }
                }
            }

            return View(model);
        }

        public ActionResult ObtenerCantones(string provincia)
        {
            List<string> cantones = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerCantonesDesdeBD";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Provincia", provincia);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            cantones.Add(reader["nombre"].ToString());
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Json(cantones, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerDistritos(string provincia, string canton)
        {
            List<string> distritos = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerDistritosDesdeBD";
                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Provincia", provincia);
                        command.Parameters.AddWithValue("@Canton", canton);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            distritos.Add(reader["nombre"].ToString());
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Json(distritos, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EnviarCorreo() {
            return View();
        }

        public ActionResult ObtenerPreguntas(string correo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("ObtenerPreguntas", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", correo);

                    RecuperarCViewModel model = new RecuperarCViewModel();
                    model.CorreoElectronico = correo;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Pregunta1 = reader["Pregunta1"].ToString();
                            model.Pregunta2 = reader["Pregunta2"].ToString();
                            model.Pregunta3 = reader["Pregunta3"].ToString();
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "El correo electrónico no está registrado.";
                            return View("EnviarCorreo");
                        }
                    }

                    TempData["CorreoElectronico"] = correo;

                    ViewBag.Correo = correo;

                    return View("RecuperarC", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al recuperar las preguntas: " + ex.Message;
                return View("EnviarCorreo");
            }
        }


        public ActionResult RecuperarC(string correo)
        {
            RecuperacionViewModel model = new RecuperacionViewModel();
            model.CorreoElectronico = correo; 
            return View(model);
        }


        [HttpPost]
        public ActionResult ValidarRespuestas(string correo, string respuesta1, string respuesta2, string respuesta3)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("CompararRespuestas", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CorreoElectronico", correo);
                    command.Parameters.AddWithValue("@Respuesta1", respuesta1);
                    command.Parameters.AddWithValue("@Respuesta2", respuesta2);
                    command.Parameters.AddWithValue("@Respuesta3", respuesta3);

                    SqlParameter resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int);
                    resultadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(resultadoParam);

                    command.ExecuteNonQuery();

                    int resultado = Convert.ToInt32(resultadoParam.Value);

                    if (resultado == 1)
                    {
                        return RedirectToAction("Recuperacion");
                    }
                    else if (resultado == 0)
                    {
                        ViewBag.ErrorMessage = "Las respuestas no son correctas. Inténtalo de nuevo.";
                        return View("RecuperarC");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Error al validar las respuestas.";
                        return View("RecuperarC");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al validar las respuestas: " + ex.Message;
                return View("RecuperarC");
            }
        }


        public ActionResult Recuperacion()
        {
            return View();
        }


        [HttpPost]
        public ActionResult CambiarContraseña(RecuperacionViewModel model)
        {
            try
            {
                string correo = TempData["CorreoElectronico"] as string;

                if (!string.IsNullOrEmpty(correo))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("ActualizarContraseñaPorCorreo", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CorreoElectronico", correo);
                        command.Parameters.AddWithValue("@Contraseña", model.NuevaContraseña);

                        command.ExecuteNonQuery();
                    }

                    ViewBag.SuccessMessage = "Contraseña cambiada exitosamente.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "No se recibió el correo electrónico.";
                    return View("Recuperacion", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cambiar la contraseña: " + ex.Message;
                return View("Recuperacion", model);
            }
        }

        public ActionResult Pujar(int subastaID)
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                PujarViewModel subasta = ObtenerDetallesSubastaDesdeBD(subastaID);

                if (subasta != null)
                {
                    return View(subasta);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult Pujar(int subastaID, decimal monto)
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                RegistrarPuja(subastaID, userID, monto, DateTime.Now);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("ActualizarPrecioActual", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@SubastaID", subastaID);
                        command.Parameters.AddWithValue("@NuevoPrecio", monto);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }



        private PujarViewModel ObtenerDetallesSubastaDesdeBD(int subastaID)
        {
            string query = "SELECT * FROM Subastas WHERE SubastaID = @SubastaID";

            PujarViewModel subasta = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SubastaID", subastaID);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        subasta = new PujarViewModel
                        {
                            SubastaID = Convert.ToInt32(reader["SubastaID"]),
                            Titulo = reader["Titulo"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            PrecioActual = Convert.ToDecimal(reader["PrecioActual"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Estado = reader["Estado"].ToString(),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            ImagenProducto = reader["ImagenProducto"].ToString()
                        };
                    }

                    reader.Close();
                }
            }

            return subasta;
        }

        private void RegistrarPuja(int subastaID, int userID, decimal monto, DateTime fechaHora)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("RegistrarPuja", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@SubastaID", subastaID);
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.Parameters.AddWithValue("@Monto", monto);
                    command.Parameters.AddWithValue("@FechaHora", fechaHora);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public ActionResult Cuenta()
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                CuentaViewModel usuario = ObtenerInformacionUsuarioDesdeBD(userID);

                if (usuario != null)
                {
                    List<SubastaViewModel> subastasPublicadas = ObtenerSubastasPorUsuarioDesdeBD(userID);

                    usuario.SubastasPublicadas = subastasPublicadas;

                    return View(usuario);
                }
            }
            return RedirectToAction("Login");
        }


        private CuentaViewModel ObtenerInformacionUsuarioDesdeBD(int userID)
        {
            CuentaViewModel usuario = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string procedimientoAlmacenado = "ObtenerInformacionUsuarioPorID";

                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", userID);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        usuario = new CuentaViewModel
                        {
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            CorreoElectronico = reader["CorreoElectronico"].ToString(),
                        };
                    }

                    reader.Close();
                }
            }

            return usuario;
        }

        private List<SubastaViewModel> ObtenerSubastasPorUsuarioDesdeBD(int userID)
        {
            List<SubastaViewModel> subastasPublicadas = new List<SubastaViewModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string procedimientoAlmacenado = "ObtenerSubastasPorUsuarioID";

                using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", userID);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        SubastaViewModel subasta = new SubastaViewModel
                        {
                            SubastaID = Convert.ToInt32(reader["SubastaID"]),
                            Titulo = reader["Titulo"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            PrecioActual = Convert.ToDecimal(reader["PrecioActual"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                            Estado = reader["Estado"].ToString(),
                            ImagenProducto = reader["ImagenProducto"].ToString()
                        };

                        subastasPublicadas.Add(subasta);
                    }

                    reader.Close();
                }
            }

            return subastasPublicadas;
        }

        public ActionResult Perfil()
        {
            return View();
        }

        public ActionResult CambiarPerfil(PerfilViewModel perfil)
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "cambiarPerfil";

                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nombre", perfil.Nombre);
                        command.Parameters.AddWithValue("@Apellido", perfil.Apellido);
                        command.Parameters.AddWithValue("@CorreoElectronico", perfil.CorreoElectronico);
                        command.Parameters.AddWithValue("@Contraseña", perfil.Contraseña);
                        command.Parameters.AddWithValue("@FotoPerfil", perfil.FotoPerfil);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult Publicar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Publicar(PublicarViewModel publicacion)
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                DateTime fechaInicioActual = DateTime.Now;
                if (fechaInicioActual < SqlDateTime.MinValue.Value || fechaInicioActual > SqlDateTime.MaxValue.Value)
                {
                    ModelState.AddModelError("", "La fecha de inicio está fuera del rango permitido.");
                    return View(publicacion); 
                }

                publicacion.FechaInicio = fechaInicioActual;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "InsertarSubasta";

                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Titulo", publicacion.Titulo);
                        command.Parameters.AddWithValue("@Descripcion", publicacion.Descripcion);
                        command.Parameters.AddWithValue("@PrecioActual", publicacion.PrecioActual);
                        command.Parameters.AddWithValue("@FechaInicio", publicacion.FechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", publicacion.FechaFin);
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@ImagenProducto", publicacion.ImagenProducto);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Cuenta", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult Ganador()
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];
                List<GanarViewModel> subastasGanadas = new List<GanarViewModel>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string procedimientoAlmacenado = "ObtenerSubastasGanadasPorUsuario";

                    using (SqlCommand command = new SqlCommand(procedimientoAlmacenado, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UsuarioID", userID);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            GanarViewModel ganador = new GanarViewModel
                            {
                                GanadorSubastaID = Convert.ToInt32(reader["GanadorSubastaID"]),
                                SubastaID = Convert.ToInt32(reader["SubastaID"]),
                                Titulo = reader["Titulo"].ToString(),
                                Precio = Convert.ToDecimal(reader["PrecioActual"]),
                                FechaGanado = Convert.ToDateTime(reader["FechaHoraGanador"]),
                                Usuario = reader["UsuarioNombre"].ToString(),
                                ImagenProducto = reader["ImagenProducto"].ToString()
                            };

                            subastasGanadas.Add(ganador);
                        }

                        reader.Close();
                    }
                }

                return View(subastasGanadas);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }


        private string ObtenerNombreUsuario(int userID)
        {
            string nombreUsuario = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Nombre, Apellido FROM Usuarios WHERE UserID = @UserID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userID);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        nombreUsuario = $"{reader["Nombre"]} {reader["Apellido"]}";
                    }

                    reader.Close();
                }
            }

            return nombreUsuario;
        }

        public ActionResult Paypal()
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                UserInfoViewModel userInfo = GetUserInfo(userID);

                decimal totalPagar = GetTotalPagar(userID);

                var model = new UserInfoViewModel
                {
                    UserInfo = userInfo,
                    TotalPagar = totalPagar
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult Tarjeta()
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                UserInfoViewModel userInfo = GetUserInfo(userID);

                decimal totalPagar = GetTotalPagar(userID);

                var model = new UserInfoViewModel
                {
                    UserInfo = userInfo,
                    TotalPagar = totalPagar
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult Transferencia()
        {
            if (Session["UserID"] != null)
            {
                int userID = (int)Session["UserID"];

                UserInfoViewModel userInfo = GetUserInfo(userID);

                decimal totalPagar = GetTotalPagar(userID);

                var model = new UserInfoViewModel
                {
                    UserInfo = userInfo,
                    TotalPagar = totalPagar
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }





        private UserInfoViewModel GetUserInfo(int userID)
        {
            UserInfoViewModel userInfo = new UserInfoViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string storedProcedure = "ObtenerInformacionUsuarioPorID";
                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserID", userID);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        userInfo.Nombre = reader["Nombre"].ToString();
                        userInfo.Apellido = reader["Apellido"].ToString();
                        userInfo.CorreoElectronico = reader["CorreoElectronico"].ToString();
                    }
                }
            }

            return userInfo;
        }

        private decimal GetTotalPagar(int userID)
        {
            decimal totalPagar = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string storedProcedure = "ObtenerTotalPagarPorUsuarioID";
                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UsuarioID", userID);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        totalPagar = Convert.ToDecimal(result);
                    }
                }
            }

            return totalPagar;
        }

    }
}
