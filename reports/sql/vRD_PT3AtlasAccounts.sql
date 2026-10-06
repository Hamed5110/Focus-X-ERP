CREATE OR ALTER VIEW dbo.vRD_PT3AtlasAccounts
AS
SELECT
    a.iMasterId,
    a.sName,
    a.sCode,
    a.CPRCRNumber,
    a.iCityName,
    a.sTelNo,
    a.SalesmannameName,
    a.DesignernameName,
    a.PipelineName,
    a.SiteStatusName,
    a.SalesModuleName,
    a.PlanValue,
    a.Planningmonth,
    a.AccountControllerName,
    a.ReportStatusName,
    CAST(0 AS decimal(18, 0)) AS iDate
FROM dbo.vmCore_Account a
WHERE a.iTreeId = 0
  AND a.ReportStatus = 3
  AND CAST(ISNULL(a.bGroup, 0) AS int) = 0
