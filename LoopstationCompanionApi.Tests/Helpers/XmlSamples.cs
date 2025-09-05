namespace LoopstationCompanionApi.Tests.Helpers;

public static class XmlSamples
{

    public const string ValidDatabaseWithIfxAndParams = """
    <database>
      <mem id="12"></mem>
      <ifx>
        <AA_LPF>
          <A>10</A>
          <B>20</B>
        </AA_LPF>
      </ifx>
    </database>
    """;

    public const string MissingDatabase = "<root></root>";

    public const string MissingIfx = "<database><mem id=\"1\"/></database>";

    public const string EmptyIfx = "<database><ifx></ifx></database>";

    public const string EmptyParamA = """
    <database>
      <ifx>
        <AA_LPF>
          <A></A>
        </AA_LPF>
      </ifx>
    </database>
    """;

    public const string AttributesNodePresent = """
    <database>
      <ifx>
        <_attributes><foo>bar</foo></_attributes>
        <AA_LPF><A>1</A></AA_LPF>
      </ifx>
    </database>
    """;

    public const string ValidImporterAA_LPF = """
    <database>
      <ifx>
        <AA_LPF>
          <A>3</A>
          <B>50</B>
          <C>50</C>
          <D>50</D>
          <E>9</E>
        </AA_LPF>
      </ifx>
    </database>
    """;

    public const string WeirdNumericAndSymbolAndCount = """
    <database>
      <count>should be removed</count>
      <ifx>
        <AA_LPF>
          <0>7</0>
          <#>8</#>
        </AA_LPF>
      </ifx>
    </database>
    """;

    public const string Clamp_AA_LPF_A_TooHigh = """
    <database>
      <ifx>
        <AA_LPF>
          <A>999</A>
        </AA_LPF>
      </ifx>
    </database>
    """;
}
