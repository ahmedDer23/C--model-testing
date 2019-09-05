namespace Linedata.Amp.Model.Automation.Helpers
{
    using System;
    using System.Data;
    using Dapper;

    public static class ModelLvHelper
    {
        public static int CreateModelFromLv(string modelName, int hierarchy, long currentUserId)
        {
            var insertedModelId = 0;
            var createModelParams = new DynamicParameters();
            createModelParams.Add("@open_model_id", insertedModelId);
            createModelParams.Add("@model_type", 0);
            createModelParams.Add("@hierarchy_id", hierarchy);
            createModelParams.Add("@owner_id", currentUserId);
            createModelParams.Add("@name", modelName);
            createModelParams.Add("@current_user", currentUserId);
            createModelParams.Add("@open_model_id", DbType.Int32, direction: ParameterDirection.Output);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Execute(
                        "create_model",
                        createModelParams,
                        trans,
                        commandType: CommandType.StoredProcedure);
                    insertedModelId = createModelParams.Get<int>("@open_model_id");

                    var closeModelParams = new DynamicParameters();
                    closeModelParams.Add("@model_id", insertedModelId);

                    connection.Execute(
                        "close_model",
                        closeModelParams,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );

                    //Work around to extract original model id instead of duplicate
                    insertedModelId--;
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return insertedModelId;
        }

        public static void ChangeLvSimpleModelName(int modelId, string modelName, long currentUserId)
        {
            var para = new DynamicParameters();
            para.Add("@model_id", modelId);
            para.Add("@name", modelName);
            para.Add("@current_user", currentUserId);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Query(
                        "update_model_properties",
                        para,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void DeleteLvSimpleModel(int modelId, long currentUserId)
        {
            var para = new DynamicParameters();
            para.Add("@model_id", modelId);
            para.Add("@current_user", currentUserId);
            para.Add("@is_Admin", 1);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Query(
                        "delete_model",
                        para,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void AddSecurityToModel(int modelId, int hierarchySectorId, long securityId, decimal weight, string positionType)
        {
            var positionTypeCode = positionType.Equals("Long", StringComparison.InvariantCultureIgnoreCase) ? 0 : 1;
            var para = new DynamicParameters();
            para.Add("@model_id", modelId);
            para.Add("@hierarchy_sector_id", hierarchySectorId);
            para.Add("@position_type_code", positionTypeCode);
            para.Add("@security_id", securityId);
            para.Add("@target", weight);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Query(
                        "update_model_target",
                        para,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    trans.Commit();
                }
                catch (Exception ex)
                { throw ex; }
            }
        }

        public static void RemoveSecurityFromModel(int modelId, int hierarchySectorId, long securityId, string positionType, int clearOverride)
        {
            var positionTypeCode = positionType.Equals("Long", StringComparison.InvariantCultureIgnoreCase) ? 0 : 1;
            var para = new DynamicParameters();
            para.Add("@model_id", modelId);
            para.Add("@hierarchy_sector_id", hierarchySectorId);
            para.Add("@position_type_code", positionTypeCode);
            para.Add("@security_id", securityId);
            para.Add("@clear_override", clearOverride);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Query(
                        "clear_model_target",
                        para,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void RemoveAllSecuritiesFromModel(int modelId)
        {
            var para = new DynamicParameters();
            para.Add("@model_id", modelId);

            using (var connection = CommonHelper.GetConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    connection.Query(
                        "clear_model_security",
                        para,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static int GetSecurityIdByName(string securityName)
        {
            var securityId = -1;
            const string query = @"select security_id from security
                    where name_1 = @securityName";

            try
            {
                using (var connection = CommonHelper.GetConnection())
                {
                    connection.Open();
                    securityId = connection.ExecuteScalar<int>(
                        query,
                        new
                        {
                            SecurityName = securityName
                        });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return securityId;
        }
    }
}

