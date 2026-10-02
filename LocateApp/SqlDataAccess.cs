using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using LocateApp.Utilities;
using LocateApp.Models;
using System.Runtime.CompilerServices;
using System.IO;
using Dapper.Transaction;

namespace LocateApp
{
    public class SqlDataAccess
    {
        private static readonly Logger Log = Logger.GetLogger(typeof(SqlDataAccess)); // pour gérer le log
        private static IDbConnection Connexion { get; } = null;

        private static IDbConnection CreateConnection(int db = 1)
        {
            string serveur, dataBase, userName, password;

            switch (db)
            {
                case 2:
                    serveur = $"Data Source={ConfigurationManager.AppSettings["ServeurMovia"]}";
                    dataBase = $"Initial Catalog={ConfigurationManager.AppSettings["DataBaseMovia"]}";
                    userName = $"User ID={ConfigurationManager.AppSettings["UserNameMovia"]}";
                    password = $"Password={ConfigurationManager.AppSettings["PasswordMovia"]}";

                    break;

                default:
                    serveur = $"Data Source={ConfigurationManager.AppSettings["Serveur"]}";
                    dataBase = $"Initial Catalog={ConfigurationManager.AppSettings["DataBase"]}";
                    userName = $"User ID={ConfigurationManager.AppSettings["UserName"]}";
                    password = $"Password={ConfigurationManager.AppSettings["Password"]}";

                    break;
            }

            //MySQL  string connString = "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=Root;";

            string connexionString = $"{serveur};{dataBase};{userName};{password};{ConfigurationManager.AppSettings["connectionString"]}";

            return new SqlConnection(connexionString);
        }

        public static IDbConnection GetConnexion(int db = 1)
        {
            string dataBase;

            switch (db)
            {
                case 2:
                    dataBase = $"Initial Catalog={ConfigurationManager.AppSettings["DataBaseMovia"]}";

                    break;

                default:
                    dataBase = $"Initial Catalog={ConfigurationManager.AppSettings["DataBase"]}";
                    break;
            }

            
            if(Connexion != null && Connexion.Database == dataBase)
            {
                return Connexion;
            }

            return CreateConnection(db);
        }

        /// <summary>Nombre d'erreurs SQL rencontrées sur le thread courant : permet de savoir si un calcul composé
        /// (tableau de bord, statistiques) s'est déroulé sans échec avant de le mettre en cache.</summary>
        [ThreadStatic] private static int nbErreursThread;
        public static int NbErreursThread => nbErreursThread;

        /// <param name="commandTimeout">Délai d'exécution SQL en secondes (null = 30 s par défaut). Réservé aux statistiques lourdes chargées en arrière-plan.</param>
        public static List<T> SelectData<T>(string sql, object parameters=null, Identity user=null, int db = 1, int? commandTimeout = null, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            // Liste vide (et non null) en cas d'échec : l'erreur est journalisée dans le catch et les appelants
            // (.FirstOrDefault(), .Count, foreach) continuent sans lever une seconde exception vers le client.
            List<T> result = new List<T>();
            string module = Path.GetFileNameWithoutExtension(file);

            using (GetConnexion(db))
            {
                try
                {

                    result = GetConnexion(db).Query<T>(sql, parameters, commandTimeout: commandTimeout).ToList();
                }
                catch (Exception e)
                {
                    nbErreursThread++;
                    Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la requête SQL : {sql} \n Valeurs : { Utilities.Utilities.GetObjectProperties(parameters)} \n , Voici le message d'erreur : {e.Message}");
                }

                return result;
               
            }
        }              

        public static int SaveData<T>(string sql, T data, Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            int result = 0;
            string module = Path.GetFileNameWithoutExtension(file);           

            using (GetConnexion(db))
            {
                try
                {
                    result = GetConnexion(db).Execute(sql, data);
                }
                catch (Exception e)
                {
                    Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la requête SQL : {sql} \n Valeurs : {Utilities.Utilities.GetObjectProperties(data)} \n , Voici le message d'erreur : {e.Message}");

                }

                return result;
            }
        }

        public static int SaveDataWithTransaction<T>(List<(string, T)> sqlAndData, Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            int result = 0;
            string module = Path.GetFileNameWithoutExtension(file);

            string sql = null;
            T data = default ;

            using (var connection = GetConnexion(db))
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {

                        foreach (var tuple in sqlAndData)
                        {
                            sql = tuple.Item1;
                            data = tuple.Item2;

                            result += transaction.Execute(sql, data);
                        }

                        transaction.Commit();

                    }

                }
                catch (Exception e)
                {
                    string resultat = result > 1? $"{result}ème" : result == 1? $"{result}ère" : "";

                    string dateInString = Utilities.Utilities.GetObjectProperties(data);

                    Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la {resultat} requête SQL  : {sql} \n Valeurs : {dateInString} \n , Voici le message d'erreur : {e.Message}");
                }

                return result;
            }
        }


        public static int SaveDataWithTransactionUsingStoreProcedure<T>(List<(string, T)> sqlAndData, Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            int result = 0;
            string module = Path.GetFileNameWithoutExtension(file);

            string sql = null;
            T data = default;

            using (var connection = GetConnexion(db))
            {
                try
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {

                        foreach (var tuple in sqlAndData)
                        {
                            sql = tuple.Item1;
                            data = tuple.Item2;

                            result += transaction.Execute(sql, data, commandType: CommandType.StoredProcedure);
                        }

                        transaction.Commit();

                    }

                }
                catch (Exception e)
                {
                    string resultat = result > 1 ? $"{result}ème" : result == 1 ? $"{result}ère" : "";
                    Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la {resultat} requête SQL  : {sql} \n Valeurs : {Utilities.Utilities.GetObjectProperties(data)} \n , Voici le message d'erreur : {e.Message}");
                }

                return result;
            }
        }


        public static List<T> LoadDataUsingStoreProcedure<T>(string sql, object parameters = null, int db = 1)
        {
            using (GetConnexion(db))
            {
                return GetConnexion(db).Query<T>(sql, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public static int SaveDataUsingStoredProcedure<T>(string sql, T data,Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            string module = Path.GetFileNameWithoutExtension(file);

            try
            {
                using (GetConnexion(db))
                {
                    return GetConnexion(db).Execute(sql, data, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception e)
            {

                Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la requête SQL : {sql} \n Valeurs : {Utilities.Utilities.GetObjectProperties(data)} \n , Voici le message d'erreur : {e.Message}");
                return 0;
            }
            
        }

        public static SqlMapper.GridReader QueryMultiple(string sql, object parameters = null, Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            string module = Path.GetFileNameWithoutExtension(file);

            try
            {
                using (GetConnexion(db))
                {
                    return GetConnexion(db).QueryMultiple(sql, parameters);
                }
            }
            catch (Exception e)
            {

                Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la requête SQL : {sql} \n Valeurs : {Utilities.Utilities.GetObjectProperties(parameters)} \n , Voici le message d'erreur : {e.Message}");
                return null;
            }

           
        }

        public static SqlMapper.GridReader QueryMultipleUsingStoredProcedure(string sql, object parameters = null, Identity user = null, int db = 1, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            string module = Path.GetFileNameWithoutExtension(file);

            try
            {
                using (GetConnexion(db))
                {
                    return GetConnexion(db).QueryMultiple(sql, parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception e)
            {

                Log.Error(user, $"({module}:{lineNumber}) Une erreur est survenue lors de l'execution de la requête SQL : {sql} \n Valeurs : {Utilities.Utilities.GetObjectProperties(parameters)} \n , Voici le message d'erreur : {e.Message}");
                return null;
            }

            
        }

    }
}