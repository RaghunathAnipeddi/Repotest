<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
  <xsl:output method="xml" omit-xml-declaration="yes"/>
  <xsl:param name="Source"/>
  <xsl:param name="Exists"/>
  <xsl:param name="Address1"/>
  <xsl:param name="Address2"/>
  <xsl:param name="Country"/>
  <xsl:param name="City"/>
  <xsl:param name="State"/>
  <xsl:param name="PostalCode"/>
  <xsl:param name="PhoneNumber"/>
  <!--<xsl:param name="Source">CustomerInformation</xsl:param>
  <xsl:param name="Exists">Yes</xsl:param>
  <xsl:param name="Address1">TEST-Address1</xsl:param>
  <xsl:param name="Address2">TEST-Address2</xsl:param>
  <xsl:param name="Country">US</xsl:param>
  <xsl:param name="City">Lake Placid</xsl:param>
  <xsl:param name="State">FL</xsl:param>
  <xsl:param name="PostalCode">425409</xsl:param>
  <xsl:param name="PhoneNumber">(987) 654-3210</xsl:param>-->

  <xsl:template match="//CustomerInformation">
    <eConnect>
      <RMCustomerMasterType>

        <xsl:if test="$Source = 'CustomerInformation' ">
          <xsl:if test="$Exists = 'Yes'">
            <taUpdateCreateCustomerRcd>
              <CUSTNMBR>
                <xsl:value-of select = "CustomerAccount/CustomerNumber"/>
              </CUSTNMBR>
              <CUSTNAME>
                <xsl:value-of select = "CustomerAccount/Name"/>
              </CUSTNAME>
              <xsl:if test ="Currency != ''">
                <CURNCYID>
                  <xsl:choose>
                    <xsl:when test ="Currency = 'USD'">
                      <xsl:text>Z-US$</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select = "CustomerAccount/Currency"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </CURNCYID>
              </xsl:if>
              <ADRSCODE>
                <xsl:text>M-</xsl:text>
                <xsl:value-of select = "CustomerAccount/CustomerNumber"/>
              </ADRSCODE>
              <!--<CNTCPRSN>
                <xsl:value-of select = "ContactName"/>
              </CNTCPRSN>-->
              <ADDRESS1>
                <xsl:value-of select = "CustomerAccount/Address/AddressLine1"/>
              </ADDRESS1>
              <ADDRESS2>
                <xsl:value-of select = "CustomerAccount/Address/AddressLine2"/>
              </ADDRESS2>
              <COUNTRY>
                <xsl:value-of select = "CustomerAccount/Address/Country/CountryID"/>
              </COUNTRY>
              <CITY>
                <xsl:value-of select = "CustomerAccount/Address/City"/>
              </CITY>
              <STATE>
                <xsl:value-of select = "CustomerAccount/Address/State/StateID"/>
              </STATE>
              <ZIPCODE>
                <xsl:value-of select = "CustomerAccount/Address/ZipCode "/>
              </ZIPCODE>
              <PHNUMBR1>
                <xsl:value-of select="normalize-space(translate(CustomerAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
              </PHNUMBR1>
              <UPSZONE>
                <xsl:value-of select = "CustomerAccount/Region/Description"/>
              </UPSZONE>
              <!--<UseCustomerClass>1</UseCustomerClass>-->
              <UpdateIfExists>
                <xsl:value-of select = "1"/>
              </UpdateIfExists>
              <CRLMTTYP>
                <xsl:choose>
                  <xsl:when test="CustomerAccount/CreditProfile/CreditType = 'Unlimited'">
                    <xsl:value-of select="1"/>
                  </xsl:when>
                  <xsl:when test="CustomerAccount/CreditProfile/CreditType = 'No Credit'">
                    <xsl:value-of select="0"/>
                  </xsl:when>
                  <xsl:when test="CustomerAccount/CreditProfile/CreditType = 'Amount'">
                    <xsl:value-of select="2"/>
                  </xsl:when>
                </xsl:choose>
                <!--<xsl:value-of select="CreditProfile/AccountCreditType"/>-->
              </CRLMTTYP>
              <xsl:choose>
                <xsl:when test="CustomerAccount/CreditProfile/CreditType = 'Amount'">
                  <CRLMTAMT>
                    <xsl:value-of select="CustomerAccount/CreditProfile/CreditLimit/Amount"/>
                  </CRLMTAMT>
                </xsl:when>
              </xsl:choose>
              <PYMTRMID>
                <xsl:choose>
                  <xsl:when test="CustomerAccount/CreditProfile/PaymentTerms = 'Pre Payment'">
                    <xsl:text>Prepayment</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select = "CustomerAccount/CreditProfile/PaymentTerms"/>
                  </xsl:otherwise>
                </xsl:choose>
                <!--<xsl:value-of select = "CreditProfile/PaymentTerms"/>-->
              </PYMTRMID>
              <INACTIVE>0</INACTIVE>
            </taUpdateCreateCustomerRcd>

            <!-- Tax Enhancement For NA -->
            <xsl:choose>
              <xsl:when test ="CustomerAccount/Region/Description='North America'">
                <taAccountTaxJurisdictionDetail_Items>
                  <xsl:for-each select="CustomerAccount/CreditProfile/TaxDetail/TaxInformations">
                    <taAccountTaxJurisdictionDetail>
                      <CustomerID>
                        <xsl:value-of select = "//CustomerAccount/CustomerNumber"/>
                      </CustomerID>
                      <NaAccountTaxJurisdictionGUID>
                        <xsl:value-of select = "TaxId"/>
                      </NaAccountTaxJurisdictionGUID>
                      <JurisdictionID>
                        <xsl:value-of select="State/StateID"/>
                      </JurisdictionID>
                      <Status>
                        <xsl:choose>
                          <xsl:when test ="Status/StatusReason='Active'">
                            <xsl:text>1</xsl:text>
                          </xsl:when>
                          <xsl:when test ="Status/StatusReason = 'Inactive'">
                            <xsl:text>0</xsl:text>
                          </xsl:when>
                        </xsl:choose>
                      </Status>
                      <Country>
                        <xsl:value-of select="Country/CountryID"/>
                      </Country>
                      <CreatedBy>
                        <xsl:value-of select = "//CustomerAccount/AuditInformation/CreatedBy"/>
                      </CreatedBy>
                      <ModifiedBy>
                        <xsl:value-of select = "//CustomerAccount/AuditInformation/ModifiedBy"/>
                      </ModifiedBy>
                    </taAccountTaxJurisdictionDetail>
                  </xsl:for-each>
                </taAccountTaxJurisdictionDetail_Items>
              </xsl:when>
            </xsl:choose>
            <!-- Tax Enhancement For EU-->
            <xsl:choose>
              <xsl:when test ="CustomerAccount/Region/Description='Europe'">
                <taAccountVATNumberReference_Items>
                  <xsl:for-each select="CustomerAccount/CreditProfile/TaxDetail/TaxInformations">
                    <taAccountVATNumberReference>
                      <CustomerID>
                        <xsl:value-of select = "//CustomerInformation/CustomerAccount/CustomerNumber"/>
                      </CustomerID>
                      <VATNumberGUID>
                        <xsl:value-of select = "TaxId"/>
                      </VATNumberGUID>
                      <Status>
                        <xsl:choose>
                          <xsl:when test ="Status/StatusReason='Active'">
                            <xsl:text>1</xsl:text>
                          </xsl:when>
                          <xsl:when test ="Status/StatusReason = 'Inactive'">
                            <xsl:text>0</xsl:text>
                          </xsl:when>
                        </xsl:choose>
                      </Status>
                      <CustomerVATNumber>
                        <xsl:value-of select = "VATNumber"/>
                      </CustomerVATNumber>
                      <CustomerVATCountry>
                        <xsl:value-of select = "Country/CountryID"/>
                      </CustomerVATCountry>
                      <CreatedBy>
                        <xsl:value-of select = "//CustomerInformation/CustomerAccount/AuditInformation/CreatedBy"/>
                      </CreatedBy>
                      <ModifiedBy>
                        <xsl:value-of select = "//CustomerInformation/CustomerAccount/AuditInformation/ModifiedBy"/>
                      </ModifiedBy>
                    </taAccountVATNumberReference>
                  </xsl:for-each>
                </taAccountVATNumberReference_Items>
              </xsl:when>
            </xsl:choose>


            <taAccountMasterReference>
              <CustomerID>
                <xsl:value-of select = "//CustomerAccount/CustomerNumber"/>
              </CustomerID>
              <CustomerGuID>
                <xsl:value-of select = "//CustomerAccount/CustomerId"/>
              </CustomerGuID>
              <CustomerStatus>
                <xsl:value-of select = "//CustomerAccount/Status/Description"/>
              </CustomerStatus>
              <CreatedBy>
                <xsl:value-of select = "//CustomerAccount/AuditInformation/CreatedBy"/>
              </CreatedBy>
              <ModifiedUser>
                <xsl:value-of select = "CustomerAccount/AuditInformation/ModifiedBy"/>
              </ModifiedUser>
              <ParentCompanyId>
                <xsl:value-of select = "//CustomerAccount/ParentCompanyId"/>
              </ParentCompanyId>
              <xsl:if test="//CustomerAccount/Region/Description='North America' and CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration!=''">
                <TaxDeclarationStatus>
                  <xsl:choose>
                    <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Taxable'">
                      <xsl:text>67</xsl:text>
                    </xsl:when>
                    <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Exempt'">
                      <xsl:text>66</xsl:text>
                    </xsl:when>
                    <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Unknown'">
                      <xsl:text>68</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text>72</xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </TaxDeclarationStatus>
              </xsl:if>
              <IsCustomerOnCollection>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/IsCustomerOnCollections"/>
              </IsCustomerOnCollection>
              <CustomerLastCollectionDate>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/CustomerLastCollectionDate"/>
              </CustomerLastCollectionDate>
              <IsCustomerOnPaymentPlan>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/IsCustomerOnPaymentPlan"/>
              </IsCustomerOnPaymentPlan>
              <LastPaymentPlanDate>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/LastPaymentPlanDate"/>
              </LastPaymentPlanDate>
              <IsCustomerOnBankrupcy>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/IsCustomerOnBankruptcy"/>
              </IsCustomerOnBankrupcy>
              <LastBankrupcyDate>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/LastBankruptcyDate"/>
              </LastBankrupcyDate>
              <IsCustomerOnCashFlowIssue>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/IsCustomerHasCashFlowIssues"/>
              </IsCustomerOnCashFlowIssue>
              <LastCashFlowIssueDate>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerRiskIndicator/LastCashFlowIssueDate"/>
              </LastCashFlowIssueDate>
              <RiskIndicatorReason></RiskIndicatorReason>
              <IsOptForPaperInvoice>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerInvoicePreference/PaperInvoice"/>
              </IsOptForPaperInvoice>
              <IsOptForElectronicInvoice>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerInvoicePreference/ElectronicInvoice"/>
              </IsOptForElectronicInvoice>
              <ElectronicInvoiceEmail>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerInvoicePreference/InvoiceCommunicationTo"/>
              </ElectronicInvoiceEmail>
              <IsPortalCustomer>
                <xsl:value-of select = "CustomerAccount/CreditProfile/CreditProfileCustomerInvoicePreference/IsPortalCustomer"/>
              </IsPortalCustomer>
            </taAccountMasterReference>
          </xsl:if>
        </xsl:if>

        <xsl:if test="$Source = 'AddressRef' ">
          <xsl:for-each select="RefAddress/AddressType">
            <taCreateCustomerAddress>
              <CUSTNMBR>
                <xsl:value-of select = "CustomerId"/>
              </CUSTNMBR>
              <ADRSCODE>
                <xsl:choose>
                  <xsl:when test ="AccountAddressTypeId = 1">
                    <xsl:text>B-</xsl:text>
                  </xsl:when>
                  <xsl:when test ="AccountAddressTypeId = 2">
                    <xsl:text>S-</xsl:text>
                  </xsl:when>
                </xsl:choose>
                <xsl:value-of select = "AccountAddressNumber"/>
              </ADRSCODE>
              <ADDRESS1>
                <xsl:value-of select = "$Address1"/>
              </ADDRESS1>
              <ADDRESS2>
                <xsl:value-of select = "$Address2"/>
              </ADDRESS2>
              <COUNTRY>
                <xsl:value-of select = "$Country"/>
              </COUNTRY>
              <CITY>
                <xsl:value-of select = "$City"/>
              </CITY>
              <STATE>
                <xsl:value-of select = "$State"/>
              </STATE>
              <ZIPCODE>
                <xsl:value-of select = "$PostalCode"/>
              </ZIPCODE>
              <PHNUMBR1>
                <xsl:value-of select="normalize-space(translate($PhoneNumber,'(;);-; ', ''))"/>
                <!--<xsl:value-of select = "$PhoneNumber"/>-->
              </PHNUMBR1>
              <UpdateIfExists>1</UpdateIfExists>
              <!--<UPSZONE>
                <xsl:value-of select="CountryCode"/>
              </UPSZONE>
              <SHIPTONAME>
                <xsl:value-of select = "CompanyName"/>
              </SHIPTONAME>-->
            </taCreateCustomerAddress>
          </xsl:for-each>
        </xsl:if>

        <xsl:if test="$Source = 'CreditProfile' and $Exists = 'Yes'">
          <xsl:for-each select="//CustomerInformation/CustomerAccount/CreditProfile">
            <taUpdateCreateCustomerRcd>
              <CUSTNMBR>
                <xsl:value-of select="//CustomerInformation/CustomerAccount/CustomerNumber"/>
              </CUSTNMBR>
              <xsl:if test ="//CustomerInformation/CustomerAccount/Currency != ''">
                <CURNCYID>
                  <xsl:choose>
                    <xsl:when test ="//CustomerInformation/CustomerAccount/Currency = 'USD'">
                      <xsl:text>Z-US$</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select = "//CustomerInformation/CustomerAccount/Currency"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </CURNCYID>
              </xsl:if>
              <PYMTRMID>
                <xsl:choose>
                  <xsl:when test="PaymentTerms = 'Pre Payment'">
                    <xsl:text>Prepayment</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select = "PaymentTerms"/>
                  </xsl:otherwise>
                </xsl:choose>
              </PYMTRMID>
              <CRLMTTYP>
                <xsl:choose>
                  <xsl:when test="CreditType = 'Unlimited'">
                    <xsl:value-of select="1"/>
                  </xsl:when>
                  <xsl:when test="CreditType = 'Amount'">
                    <xsl:value-of select="2"/>
                  </xsl:when>
                  <xsl:when test="CreditType = 'No Credit'">
                    <xsl:value-of select="0"/>
                  </xsl:when>
                </xsl:choose>
                <!-- <xsl:value-of select="AccountCreditType"/>-->
              </CRLMTTYP>
              <xsl:choose>
                <xsl:when test="CreditType = 'Amount'">
                  <CRLMTAMT>
                    <xsl:value-of select="CreditLimit/Amount"/>
                  </CRLMTAMT>
                </xsl:when>
              </xsl:choose>
              <!--<CRLMTPER>
                <xsl:value-of select="CreditLimitPrcnt"/>
              </CRLMTPER>
              <CRLMTPAM>
                <xsl:value-of select="CreditLimitPerAmt"/>
              </CRLMTPAM>-->
              <!--<CUSTCLAS>
                <xsl:value-of select = "CustomerClassID"/>
              </CUSTCLAS>-->
            </taUpdateCreateCustomerRcd>
          </xsl:for-each>
<taAccountMasterReference>
          <CustomerID>
            <xsl:value-of select = "CustomerAccount/CustomerNumber"/>
          </CustomerID>
          <CustomerGuID>
            <xsl:value-of select = "CustomerAccount/CustomerId"/>
          </CustomerGuID>
          <CustomerStatus>
            <xsl:value-of select = "CustomerAccount/Status/Description"/>
          </CustomerStatus>
          <CreatedBy>
            <xsl:value-of select = "CustomerAccount/AuditInformation/CreatedBy"/>
          </CreatedBy>
          <ModifiedUser>
            <xsl:value-of select = "CustomerAccount/AuditInformation/ModifiedBy"/>
          </ModifiedUser>
          <ParentCompanyId>
            <xsl:value-of select = "CustomerAccount/ParentCompanyId"/>
          </ParentCompanyId>
          <xsl:if test="//CustomerAccount/Region/Description='North America' and CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration!=''">
            <TaxDeclarationStatus>
              <xsl:choose>
                <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Taxable'">
                  <xsl:text>67</xsl:text>
                </xsl:when>
                <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Exempt'">
                  <xsl:text>66</xsl:text>
                </xsl:when>
                <xsl:when  test="CustomerAccount/CreditProfile/TaxDetail/CustomerDeclaration='Unknown'">
                  <xsl:text>68</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>72</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </TaxDeclarationStatus>
          </xsl:if>
        </taAccountMasterReference>
<!-- Tax Enhancement For NA -->
            <xsl:choose>
              <xsl:when test ="CustomerAccount/Region/Description='North America'">
                <taAccountTaxJurisdictionDetail_Items>
                  <xsl:for-each select="CustomerAccount/CreditProfile/TaxDetail/TaxInformations">
                    <taAccountTaxJurisdictionDetail>
                      <CustomerID>
                        <xsl:value-of select = "//CustomerAccount/CustomerNumber"/>
                      </CustomerID>
                      <NaAccountTaxJurisdictionGUID>
                        <xsl:value-of select = "TaxId"/>
                      </NaAccountTaxJurisdictionGUID>
                      <JurisdictionID>
                        <xsl:value-of select="State/StateID"/>
                      </JurisdictionID>
                      <Status>
                        <xsl:choose>
                          <xsl:when test ="Status/StatusReason='Active'">
                            <xsl:text>1</xsl:text>
                          </xsl:when>
                          <xsl:when test ="Status/StatusReason = 'Inactive'">
                            <xsl:text>0</xsl:text>
                          </xsl:when>
                        </xsl:choose>
                      </Status>
                      <Country>
                        <xsl:value-of select="Country/CountryID"/>
                      </Country>
                      <CreatedBy>
                        <xsl:value-of select = "//CustomerAccount/AuditInformation/CreatedBy"/>
                      </CreatedBy>
                      <ModifiedBy>
                        <xsl:value-of select = "//CustomerAccount/AuditInformation/ModifiedBy"/>
                      </ModifiedBy>
                    </taAccountTaxJurisdictionDetail>
                  </xsl:for-each>
                </taAccountTaxJurisdictionDetail_Items>
              </xsl:when>
            </xsl:choose>
        </xsl:if> 
      </RMCustomerMasterType>
    </eConnect>
  </xsl:template>
</xsl:stylesheet>


