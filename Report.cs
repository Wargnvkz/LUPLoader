using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace LUPLoader
{
    public static class Report
    {
        static object lockobject = new object();
        static Report()
        {
            AppDomain.CurrentDomain.ProcessExit += StopReport;
        }
        public static void AddCommand(UPMCommand command)
        {
            if (command.Command != UPMCommandType.GranulateLoad) return;
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);

                try
                {
                    var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(cs))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO BagsLoadedCommand(Loaded,Material,LUP,BagsCount) VALUES(@Loaded,@Material,@LUP,@BagsCount)", connection);

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@Loaded", DateTime.Now);
                        cmd.Parameters.Add("@Material", command.Material);
                        cmd.Parameters.Add("@LUP", command.LUP);
                        cmd.Parameters.Add("@BagsCount", command.BagQuant);


                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.Add("Ошибка при записи в базу данных. Material: " + command.Material + ", LUP: " + command.LUP + ", BagsCount: " + command.BagQuant, true, 0);
                    Log.Add(ex);
                }
            }
        }

        public static void AddLUPAtShiftStart(DateTime DateShift, bool IsNight, int LUP1Weight, int LUP2Weight, int LUP3Weight)
        {
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);

                try
                {
                    var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(cs))
                    {

                        connection.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO LUPAtShiftStart(DateShift,IsNight,LUP1Weight,LUP2Weight,LUP3Weight) VALUES(@DateShift,@IsNight,@LUP1Weight,@LUP2Weight,@LUP3Weight)", connection);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@DateShift", DateShift);
                        cmd.Parameters.Add("@IsNight", IsNight);
                        cmd.Parameters.Add("@LUP1Weight", LUP1Weight);
                        cmd.Parameters.Add("@LUP2Weight", LUP2Weight);
                        cmd.Parameters.Add("@LUP3Weight", LUP3Weight);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.Add("Ошибка при записи в базу данных. LUP1: " + LUP1Weight + ", LUP2: " + LUP2Weight, true, 0);
                    Log.Add(ex);
                }
            }
        }

        public static void AddBagLoaded(DateTime Loaded, string Material, int lup, double BagWeight, string BagHU)
        {
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);

                try
                {
                    var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(cs))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO BagsLoaded(Loaded,Material,LUP,Weight,HU) VALUES(@Loaded,@Material,@LUP,@Weight,@HU)", connection);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@Loaded", Loaded);
                        cmd.Parameters.Add("@Material", Material);
                        cmd.Parameters.Add("@LUP", lup);
                        cmd.Parameters.Add("@Weight", BagWeight);
                        cmd.Parameters.Add("@HU", BagHU);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.Add("Ошибка при записи в базу данных. Material: " + Material + ", LUP: " + lup + ", Weight: " + BagWeight + ", HU: " + BagHU, true, 0);
                    Log.Add(ex);
                }
            }
        }

        public static void AddMaterialAtShiftStart(DateTime shift, bool IsNight, List<UPMAction.MaterialLeft> ml)
        {
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);


                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                SqlTransaction trans = null;



                try
                {
                    using (SqlConnection connection = new SqlConnection(cs))
                    {
                        connection.Open();

                        Log.Add("Запись материалов на начало смены " + shift.ToShortDateString() + " " + (IsNight ? "Н" : "Д"), true, 0);
                        trans = connection.BeginTransaction();

                        foreach (var m in ml)
                        {
                            SqlCommand cmd = new SqlCommand("insert into MaterialsAtShiftStart(DateShift,IsNight,Material,FullWeight,BagCount,BagWeight) VALUES(@DateShift,@IsNight,@Material,@FullWeight,@BagCount,@BagWeight)", connection, trans);
                            cmd.Parameters.Add("@DateShift", shift);
                            cmd.Parameters.Add("@IsNight", IsNight);
                            cmd.Parameters.Add("@Material", m.Material);
                            cmd.Parameters.Add("@FullWeight", m.Quant);
                            cmd.Parameters.Add("@BagCount", m.BagCount);
                            cmd.Parameters.Add("@BagWeight", m.BaseWeight);
                            cmd.ExecuteNonQuery();
                            Log.Add("Материал: " + m.Material + ". Общий вес: " + m.Quant + ". Количество мешков: " + m.BagCount + ". Вес мешка: " + m.BaseWeight, true, 0);
                        }

                        trans.Commit();
                        Log.Add("Запись закончена. Транзакция подтверждена", true, 0);
                    }
                }
                catch (Exception ex) //error occurred
                {
                    if (trans != null)
                        trans.Rollback();
                    Log.Add("Транзакция отменена", true, 0);
                    Log.Add(ex);
                    //Handel error
                }

                /*
                 *                 SqlCommand cmd = new SqlCommand();

                StringBuilder sb=new StringBuilder();
                sb.AppendLine("insert into UPM_MaterialAtShiftStart(DateShift,IsNight,Material,FullWeight,BagCount,BagWeight) VALUES");
                List<string> values = new List<string>();
                for(int i=0;i<ml.Count;i++)
                {
                    var m=ml[i];
                    
                    cmd.CommandType = CommandType.Text;

                    values.Add("(@DateShift"+i+",@IsNight"+i+",@Material"+i+",@FullWeight"+i+",@BagCount"+i+",@BagWeight"+i+")");

                    cmd.Parameters.Add("@DateShift"+i, shift);
                    cmd.Parameters.Add("@IsNight"+i, IsNight);
                    cmd.Parameters.Add("@Material"+i, m.Material);
                    cmd.Parameters.Add("@FullWeight"+i, m.Quant);
                    cmd.Parameters.Add("@BagCount"+i, m.BagCount);
                    cmd.Parameters.Add("@BagWeight"+i, m.BaseWeight);

                }

                cmd.ExecuteNonQuery();*/
            }
        }

        public static void MaprDuoBagsCorrections(DateTime shift, bool IsNight, List<Correction> Corrections, List<UPMAction.HU> HU_list)
        {
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);

                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                SqlTransaction trans = null;

                var hugroup=HU_list.GroupBy(h => new { h.MaterialNumber, h.Quantity });
                var income=hugroup.Select(hg => new Correction() { Material = hg.Key.MaterialNumber.Trim().TrimStart('0'), BagWeight = Convert.ToInt32(hg.Key.Quantity), Income = hg.Count() }).ToList();
                var income_temp = new List<Correction>();
                income_temp.AddRange(income);
                var bags_corrections = new List<Bags_Correction>();
                foreach (var c in Corrections)
                {
                    var bc = new Bags_Correction();
                    var income_mat = income.Find(i => i.BagWeight == c.BagWeight && c.Material == i.Material);
                    if (income_mat != null)
                    {
                        bc.Income = income_mat.Income;
                        income_temp.Remove(income_mat);
                    }
                    else
                    {
                        bc.Income = 0;
                    }
                    bc.Material = c.Material;
                    bc.BagWeight = c.BagWeight;
                    bc.AtShiftStart = c.AtShiftStart;
                    bc.Outgo = c.Outgo;
                    bc.AtShiftEnd = c.AtShiftEnd;
                    bc.CorrectionValue = c.CorrectionValue;
                    bc.CorrectionText = c.CorrectionText;
                    bags_corrections.Add(bc);
                }

                foreach(var income_mat in income_temp)
                {
                    var bc = new Bags_Correction();
                    bc.Income = income_mat.Income;
                    bc.Material = income_mat.Material;
                    bc.BagWeight = income_mat.BagWeight;
                    bc.AtShiftStart = 0;
                    bc.Outgo = 0;
                    bc.AtShiftEnd = income_mat.Income;
                    bc.CorrectionValue = 0;
                    bc.CorrectionText = "Материала нет в списке MaprDuo";
                    bags_corrections.Add(bc);
                }

                /*var bags_corrections_r = (from c in Corrections
                                       join hu in income on c.Material equals hu.Material into hu_temp
                                       from h_income in hu_temp.DefaultIfEmpty()
                                       select new Bags_Correction() { Material = c.Material, BagWeight = c.BagWeight, BagQuantity = c.BagQuantity, CorrectionValue = c.CorrectionValue, CorrectionText = c.CorrectionText, Income = h_income==null?0:h_income.BagQuantity }).ToList();
                
                var bags_corrections_l = (from h_income in income
                                         join c in Corrections on h_income.Material equals c.Material into c_temp
                                         from c in c_temp.DefaultIfEmpty()
                                         select new Bags_Correction() { Material = h_income.Material, BagWeight = h_income.BagWeight, BagQuantity = h_income.BagQuantity, CorrectionValue = c==null?(short)0:c.CorrectionValue, CorrectionText = c == null ? "" : c.CorrectionText??"", Income = h_income == null ? 0 : h_income.BagQuantity }).ToList();
                var bags_corrections = bags_corrections_r.Union(bags_corrections_l).ToList();*/

                try
                {
                    using (SqlConnection connection = new SqlConnection(cs))
                    {
                        connection.Open();

                        Log.Add("Запись движения гранулята в смене " + shift.ToShortDateString() + " " + (IsNight ? "Н" : "Д"), true, 0);
                        trans = connection.BeginTransaction();

                        foreach (var corr in bags_corrections)
                        {
                            //SqlCommand cmd = new SqlCommand("insert into CorrectionsAtShiftEnd(DateShift,IsNight,Material,BagWeight,Income,BagQuantity,CorrectionValue,CorrectionText) VALUES(@DateShift,@IsNight,@Material,@BagWeight,@Income,@BagQuantity,@CorrectionValue,@CorrectionText)", connection, trans);
                            //cmd.CommandType = CommandType.Text;
                            SqlCommand cmd = new SqlCommand(@"INSERT INTO [dbo].[MaprDuoShiftStatistics]
           ([DateShift]
           ,[IsNight]
           ,[Material]
           ,[BagWeight]
           ,[BagQuantity_Start]
           ,[Income]
           ,[Loaded]
           ,[BagQuantity_End]
           ,[CorrectionValue]
           ,[CorrectionText])
         
     VALUES
           (@DateShift
           ,@IsNight
           ,@Material
           ,@BagWeight
           ,@AtShiftStart
           ,@Income
           ,@Loaded
           ,@AtShiftEnd
           ,@CorrectionValue
           ,@CorrectionText)", connection, trans);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@DateShift", shift);
                            cmd.Parameters.Add("@IsNight", IsNight);
                            cmd.Parameters.Add("@Material", corr.Material);
                            cmd.Parameters.Add("@BagWeight", corr.BagWeight);
                            cmd.Parameters.Add("@Income", corr.Income);
                            //cmd.Parameters.Add("@BagQuantity", corr.BagQuantity);
                            cmd.Parameters.Add("@AtShiftStart",  corr.AtShiftStart);
                            cmd.Parameters.Add("@Loaded",  corr.Outgo);
                            cmd.Parameters.Add("@AtShiftEnd", corr.AtShiftEnd);
                            cmd.Parameters.Add("@CorrectionValue", corr.CorrectionValue);
                            cmd.Parameters.Add("@CorrectionText", corr.CorrectionText);
                            cmd.ExecuteNonQuery();
                            Log.Add(
                                String.Format("Смена: {0}{1}, Материал: {2}, Вес мешка: {3}, На начало: {4} Приход: {5} Расход: {6} Конец: {7} Поправка: {8} Текст поправки: {9}", shift, IsNight ? "Н" : "Д", corr.Material, corr.BagWeight, corr.Income, corr.AtShiftStart, corr.Outgo, corr.AtShiftEnd, corr.CorrectionValue, corr.CorrectionText)
                                , true, 0); 
                        }

                        trans.Commit();
                        Log.Add("Запись закончена. Транзакция подтверждена", true, 0);
                    }
                }
                catch (Exception ex) //error occurred
                {
                    Log.Add(ex);
                    Log.Add("Отмена транзакции", true, 0);
                    if (trans != null)
                        trans.Rollback();
                    Log.Add("Транзакция отменена", true, 0);
                    //Handel error
                }


            }
        }
        public static void MaprDuoLUPCorrections(DateTime shift, bool IsNight, List<Correction> Corrections)
        {
            lock (lockobject)
            {
                var selectedLanguage = "ru-RU";
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(selectedLanguage);

                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                SqlTransaction trans = null;

                /*var hugroup = HU_list.GroupBy(h => new { h.MaterialNumber, h.Quantity });
                var income = hugroup.Select(hg => new Correction() { Material = hg.Key.MaterialNumber.Trim().TrimStart('0'), BagWeight = Convert.ToInt32(hg.Key.Quantity), Income = hg.Count() }).ToList();
                var income_temp = new List<Correction>();
                income_temp.AddRange(income);*/
                var LUP_corrections = new List<LUP_Correction>();
                foreach (var c in Corrections)
                {
                    var lc = new LUP_Correction();
                    /*var income_mat = income.Find(i => i.BagWeight == c.BagWeight && c.Material == i.Material);
                    if (income_mat != null)
                    {
                        bc.Income = income_mat.Income;
                        income_temp.Remove(income_mat);
                    }
                    else
                    {
                        bc.Income = 0;
                    }*/
                    lc.LUP = c.LUP;
                    lc.Material = c.Material;
                    lc.AtShiftStart = c.AtShiftStart;
                    lc.Income = c.Income;
                    lc.Outgo = c.Outgo;
                    lc.AtShiftEnd = c.AtShiftEnd;
                    lc.CorrectionValue = c.CorrectionValue;
                    lc.CorrectionText = c.CorrectionText;
                    LUP_corrections.Add(lc);
                }

                /*foreach (var income_mat in income_temp)
                {
                    var bc = new LUP_Correction();
                    bc.Income = income_mat.Income;
                    bc.Material = income_mat.Material;
                    bc.BagWeight = income_mat.BagWeight;
                    bc.AtShiftStart = 0;
                    bc.Outgo = 0;
                    bc.AtShiftEnd = income_mat.Income;
                    bc.CorrectionValue = 0;
                    bc.CorrectionText = "Материала нет в списке MaprDuo";
                    LUP_corrections.Add(bc);
                }*/

                /*var bags_corrections_r = (from c in Corrections
                                       join hu in income on c.Material equals hu.Material into hu_temp
                                       from h_income in hu_temp.DefaultIfEmpty()
                                       select new Bags_Correction() { Material = c.Material, BagWeight = c.BagWeight, BagQuantity = c.BagQuantity, CorrectionValue = c.CorrectionValue, CorrectionText = c.CorrectionText, Income = h_income==null?0:h_income.BagQuantity }).ToList();
                
                var bags_corrections_l = (from h_income in income
                                         join c in Corrections on h_income.Material equals c.Material into c_temp
                                         from c in c_temp.DefaultIfEmpty()
                                         select new Bags_Correction() { Material = h_income.Material, BagWeight = h_income.BagWeight, BagQuantity = h_income.BagQuantity, CorrectionValue = c==null?(short)0:c.CorrectionValue, CorrectionText = c == null ? "" : c.CorrectionText??"", Income = h_income == null ? 0 : h_income.BagQuantity }).ToList();
                var bags_corrections = bags_corrections_r.Union(bags_corrections_l).ToList();*/

                try
                {
                    using (SqlConnection connection = new SqlConnection(cs))
                    {
                        connection.Open();

                        Log.Add("Запись движений гранулята в линиях в смене " + shift.ToShortDateString() + " " + (IsNight ? "Н" : "Д"), true, 0);
                        trans = connection.BeginTransaction();

                        foreach (var corr in LUP_corrections)
                        {
                            //SqlCommand cmd = new SqlCommand("insert into CorrectionsAtShiftEnd(DateShift,IsNight,Material,BagWeight,Income,BagQuantity,CorrectionValue,CorrectionText) VALUES(@DateShift,@IsNight,@Material,@BagWeight,@Income,@BagQuantity,@CorrectionValue,@CorrectionText)", connection, trans);
                            //cmd.CommandType = CommandType.Text;
                            SqlCommand cmd = new SqlCommand(@"INSERT INTO [dbo].[MaprDuoLUPShiftStatistics]
           ([DateShift]
           ,[IsNight]
           ,[LUP]
           ,[Material]
           ,[LUPAtShiftStart]
           ,[Income]
           ,[Consumption]
           ,[LUPAtShiftEnd]
           ,[CorrectionValue]
           ,[CorrectionText])
         
     VALUES
           (@DateShift
           ,@IsNight
           ,@LUP
           ,@Material
           ,@LUPAtShiftStart
           ,@Income
           ,@Consumption
           ,@LUPAtShiftEnd
           ,@CorrectionValue
           ,@CorrectionText)", connection, trans);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@DateShift", shift);
                            cmd.Parameters.Add("@IsNight", IsNight);
                            cmd.Parameters.Add("@LUP", corr.LUP);
                            cmd.Parameters.Add("@Material", corr.Material);
                            //cmd.Parameters.Add("@BagQuantity", corr.BagQuantity);
                            cmd.Parameters.Add("@LUPAtShiftStart", corr.AtShiftStart);
                            cmd.Parameters.Add("@Income", corr.Income);
                            cmd.Parameters.Add("@Consumption", corr.Outgo);
                            cmd.Parameters.Add("@LUPAtShiftEnd", corr.AtShiftEnd);
                            cmd.Parameters.Add("@CorrectionValue", corr.CorrectionValue);
                            cmd.Parameters.Add("@CorrectionText", corr.CorrectionText);
                            cmd.ExecuteNonQuery();
                            Log.Add(
                                String.Format("Смена: {0}{1}, LUP: {2}, Материал: {3}, На начало: {4} Приход: {5} Расход: {6} Конец: {7} Поправка: {8} Текст поправки: {9}", shift, IsNight ? "Н" : "Д", corr.LUP, corr.Material, corr.Income, corr.AtShiftStart, corr.Outgo, corr.AtShiftEnd, corr.CorrectionValue, corr.CorrectionText)
                                , true, 0);
                        }

                        trans.Commit();
                        Log.Add("Запись закончена. Транзакция подтверждена", true, 0);
                    }
                }
                catch (Exception ex) //error occurred
                {
                    Log.Add(ex);
                    Log.Add("Отмена транзакции", true, 0);
                    if (trans != null)
                        trans.Rollback();
                    Log.Add("Транзакция отменена", true, 0);
                    //Handel error
                }


            }
        }
        static void StopReport(object sender, EventArgs e)
        {
            try
            {
            }
            finally
            {
            }
        }


    }

    internal class Bags_Correction
    {
        public string Material;
        public int BagWeight;
        public int AtShiftStart;
        public int Income;
        public int Outgo;
        public int AtShiftEnd;
        public int CorrectionValue;
        public string CorrectionText;
    }
    internal class LUP_Correction
    {
        public int LUP;
        public string Material;
        public int AtShiftStart;
        public int Income;
        public int Outgo;
        public int AtShiftEnd;
        public int CorrectionValue;
        public string CorrectionText;
    }
}
