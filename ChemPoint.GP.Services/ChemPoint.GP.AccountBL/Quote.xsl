<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
  <xsl:output method="xml" omit-xml-declaration="yes"/>
  <xsl:param name="IsCustomerExists"/>
  <!--<xsl:param name="IsCustomerExists">Yes</xsl:param>-->
  <xsl:template match="//QuoteInformation">
    <xsl:variable name="FinalDestinationAccount" select="FinalDestinationAccount/CustomerNumber"/>
    <eConnect>
      <RMCustomerMasterType>
        <xsl:if test="$IsCustomerExists = 'No'">
          <taUpdateCreateCustomerRcd>
            <CUSTNMBR>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CUSTNMBR>
            <CUSTNAME>
              <xsl:value-of select = "MainAccount/Name"/>
            </CUSTNAME>
            <xsl:if test ="Currency != ''">
              <CURNCYID>
                <xsl:choose>
                  <xsl:when test ="Currency = 'USD'">
                    <xsl:text>Z-US$</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select = "Currency"/>
                  </xsl:otherwise>
                </xsl:choose>
              </CURNCYID>
            </xsl:if>
            <CUSTCLAS>STANDARD</CUSTCLAS>
            <ADRSCODE>
              <xsl:text>M-</xsl:text>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </ADRSCODE>
            <ADDRESS1>
              <xsl:value-of select = "MainAccount/Address/AddressLine1"/>
            </ADDRESS1>
            <ADDRESS2>
              <xsl:value-of select = "MainAccount/Address/AddressLine2"/>
            </ADDRESS2>
            <COUNTRY>
              <xsl:value-of select = "MainAccount/Address/Country/CountryID"/>
            </COUNTRY>
            <CITY>
              <xsl:value-of select = "MainAccount/Address/City"/>
            </CITY>
            <STATE>
              <xsl:value-of select = "MainAccount/Address/State/StateID"/>
            </STATE>
            <ZIPCODE>
              <xsl:value-of select = "MainAccount/Address/ZipCode"/>
            </ZIPCODE>
            <PHNUMBR1>
              <xsl:value-of select="normalize-space(translate(MainAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
            </PHNUMBR1>
            <UPSZONE>
              <xsl:value-of select = "LineItem/Sku/Region"/>
            </UPSZONE>
            <xsl:if test="LineItem/Sku/Region!=''">
              <xsl:choose>
                <xsl:when test="LineItem/Sku/Region='North America'">
                  <TAXSCHID>
                    <xsl:text>AVATAX</xsl:text>
                  </TAXSCHID>
                </xsl:when>
              </xsl:choose>
            </xsl:if>

            <!--<UseCustomerClass>1</UseCustomerClass>-->
            <CRLMTTYP>
              <xsl:choose>
                <xsl:when test="MainAccount/CreditProfile/CreditType = 'Unlimited'">
                  <xsl:value-of select="1"/>
                </xsl:when>
                <xsl:when test="MainAccount/CreditProfile/CreditType = 'No Credit'">
                  <xsl:value-of select="0"/>
                </xsl:when>
                <xsl:when test="MainAccount/CreditProfile/CreditType = 'Amount'">
                  <xsl:value-of select="2"/>
                </xsl:when>
              </xsl:choose>
            </CRLMTTYP>
            <xsl:choose>
              <xsl:when test="MainAccount/CreditProfile/CreditType = 'Amount'">
                <CRLMTAMT>
                  <xsl:value-of select="MainAccount/CreditProfile/CreditLimit/Amount"/>
                </CRLMTAMT>
              </xsl:when>
            </xsl:choose>
            <PYMTRMID>
              <xsl:choose>
                <xsl:when test="MainAccount/CreditProfile/PaymentTerms = 'Pre Payment'">
                  <xsl:text>Prepayment</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select = "MainAccount/CreditProfile/PaymentTerms"/>
                </xsl:otherwise>
              </xsl:choose>
            </PYMTRMID>
            <INACTIVE>0</INACTIVE>
          </taUpdateCreateCustomerRcd>
          <taAccountMasterReference>
            <CustomerID>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CustomerID>
            <CustomerGuID>
              <xsl:value-of select = "MainAccount/CustomerId"/>
            </CustomerGuID>
            <CustomerStatus>
              <xsl:value-of select = "MainAccount/CustomerStatus"/>
            </CustomerStatus>
            <CreatedBy>
              <xsl:value-of select = "AuditInformation/CreatedBy"/>
            </CreatedBy>
            <ModifiedUser>
              <xsl:value-of select = "AuditInformation/ModifiedBy"/>
            </ModifiedUser>
            <ParentCompanyId>
              <xsl:value-of select = "MainAccount/ParentCompanyId"/>
            </ParentCompanyId>
            <xsl:if test="//QuoteInformation/MainAccount/Region/Description='North America' and //QuoteInformation/MainAccount/Region/Description!=''">
              <TaxDeclarationStatus>
                <xsl:choose>
                  <xsl:when  test="QuoteInformation/MainAccount/CreditProfile/TaxDetail/CustomerDeclaration='Taxable'">
                    <xsl:text>67</xsl:text>
                  </xsl:when>
                  <xsl:when  test="QuoteInformation/MainAccount/CreditProfile/TaxDetail/CustomerDeclaration='Exempt'">
                    <xsl:text>66</xsl:text>
                  </xsl:when>
                  <xsl:when  test="QuoteInformation/MainAccount/CreditProfile/TaxDetail/CustomerDeclaration='Unknown'">
                    <xsl:text>68</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>72</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </TaxDeclarationStatus>
            </xsl:if>
            <IsCustomerOnCollection>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/IsCustomerOnCollections"/>
            </IsCustomerOnCollection>
            <CustomerLastCollectionDate>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/CustomerLastCollectionDate"/>
            </CustomerLastCollectionDate>
            <IsCustomerOnPaymentPlan>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/IsCustomerOnPaymentPlan"/>
            </IsCustomerOnPaymentPlan>
            <LastPaymentPlanDate>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/LastPaymentPlanDate"/>
            </LastPaymentPlanDate>
            <IsCustomerOnBankrupcy>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/IsCustomerOnBankruptcy"/>
            </IsCustomerOnBankrupcy>
            <LastBankrupcyDate>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/LastBankruptcyDate"/>
            </LastBankrupcyDate>
            <IsCustomerOnCashFlowIssue>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/IsCustomerHasCashFlowIssues"/>
            </IsCustomerOnCashFlowIssue>
            <LastCashFlowIssueDate>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerRiskIndicator/LastCashFlowIssueDate"/>
            </LastCashFlowIssueDate>
            <RiskIndicatorReason>
              
            </RiskIndicatorReason>
            <IsOptForPaperInvoice>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerInvoicePreference/PaperInvoice"/>
            </IsOptForPaperInvoice>
            <IsOptForElectronicInvoice>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerInvoicePreference/ElectronicInvoice"/>
            </IsOptForElectronicInvoice>
            <ElectronicInvoiceEmail>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerInvoicePreference/InvoiceCommunicationTo"/>
            </ElectronicInvoiceEmail>
            <IsPortalCustomer>
              <xsl:value-of select = "QuoteInformation/MainAccount/CreditProfile/CustomerInvoicePreference/IsPortalCustomer"/>
            </IsPortalCustomer>
          </taAccountMasterReference>

          <!-- Tax Enhancement For NA -->
          <xsl:choose>
            <xsl:when test ="QuoteInformation/MainAccount/Region/Description='North America'">
              <taAccountTaxJurisdictionDetail_Items>
                <xsl:for-each select="QuoteInformation/MainAccount/CreditProfile/TaxDetail/TaxInformations">
                  <taAccountTaxJurisdictionDetail>
                    <CustomerID>
                      <xsl:value-of select = "//MainAccount/CustomerNumber"/>
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
                      <xsl:value-of select = "AuditInformation/CreatedBy"/>
                    </CreatedBy>
                    <ModifiedBy>
                      <xsl:value-of select = "AuditInformation/ModifiedBy"/>
                    </ModifiedBy>
                  </taAccountTaxJurisdictionDetail>
                </xsl:for-each>
              </taAccountTaxJurisdictionDetail_Items>
            </xsl:when>
          </xsl:choose>
          
           <!--Tax Enhancement For EU-->
          <xsl:choose>
            <xsl:when test ="//QuoteInformation/MainAccount/Region/Description='Europe'">
              <taAccountVATNumberReference_Items>
                <xsl:for-each select="//QuoteInformation/MainAccount/CreditProfile/TaxDetail/TaxInformations">
                  <taAccountVATNumberReference>
                    <CustomerID>
                      <xsl:value-of select = "//MainAccount/CustomerNumber"/>
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
                      <xsl:value-of select = "AuditInformation/CreatedBy"/>
                    </CreatedBy>
                    <ModifiedBy>
                      <xsl:value-of select = "AuditInformation/ModifiedBy"/>
                    </ModifiedBy>
                  </taAccountVATNumberReference>
                </xsl:for-each>
              </taAccountVATNumberReference_Items>
            </xsl:when>
          </xsl:choose>
          <taCreateCustomerAddress>
            <CUSTNMBR>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CUSTNMBR>
            <ADRSCODE>
              <xsl:text>B-</xsl:text>
              <xsl:value-of select = "BillingAccount/CustomerNumber"/>
            </ADRSCODE>
            <xsl:if test="LineItem/Sku/Region!=''">
              <xsl:choose>
                <xsl:when test="LineItem/Sku/Region='North America'">
                  <TAXSCHID>
                    <xsl:text>AVATAX</xsl:text>
                  </TAXSCHID>
                </xsl:when>
              </xsl:choose>
            </xsl:if>
            <ADDRESS1>
              <xsl:value-of select = "BillingAccount/Address/AddressLine1"/>
            </ADDRESS1>
            <ADDRESS2>
              <xsl:value-of select = "BillingAccount/Address/AddressLine2"/>
            </ADDRESS2>
            <COUNTRY>
              <xsl:value-of select = "BillingAccount/Address/Country/CountryID"/>
            </COUNTRY>
            <CITY>
              <xsl:value-of select = "BillingAccount/Address/City"/>
            </CITY>
            <STATE>
              <xsl:value-of select = "BillingAccount/Address/State/StateID"/>
            </STATE>
            <ZIPCODE>
              <xsl:value-of select = "BillingAccount/Address/ZipCode"/>
            </ZIPCODE>
            <PHNUMBR1>
              <xsl:value-of select="normalize-space(translate(BillingAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
            </PHNUMBR1>
            <UPSZONE>
              <xsl:value-of select="LineItem/Sku/Region"/>
            </UPSZONE>
            <ShipToName>
              <xsl:value-of select="BillingAccount/Name"/>
            </ShipToName>
          </taCreateCustomerAddress>
          <taCreateCustomerAddress>
            <CUSTNMBR>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CUSTNMBR>
            <ADRSCODE>
              <xsl:text>S-</xsl:text>
              <xsl:value-of select = "ShippingAccount/CustomerNumber"/>
            </ADRSCODE>
            <xsl:if test="LineItem/Sku/Region!=''">
              <xsl:choose>
                <xsl:when test="LineItem/Sku/Region='North America'">
                  <TAXSCHID>
                    <xsl:text>AVATAX</xsl:text>
                  </TAXSCHID>
                </xsl:when>
              </xsl:choose>
            </xsl:if>
            <ADDRESS1>
              <xsl:value-of select = "ShippingAccount/Address/AddressLine1"/>
            </ADDRESS1>
            <ADDRESS2>
              <xsl:value-of select = "ShippingAccount/Address/AddressLine2"/>
            </ADDRESS2>
            <COUNTRY>
              <xsl:value-of select = "ShippingAccount/Address/Country/CountryID"/>
            </COUNTRY>
            <CITY>
              <xsl:value-of select = "ShippingAccount/Address/City"/>
            </CITY>
            <STATE>
              <xsl:value-of select = "ShippingAccount/Address/State/StateID"/>
            </STATE>
            <ZIPCODE>
              <xsl:value-of select = "ShippingAccount/Address/ZipCode"/>
            </ZIPCODE>
            <PHNUMBR1>
              <xsl:value-of select="normalize-space(translate(ShippingAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
            </PHNUMBR1>
            <UPSZONE>
              <xsl:value-of select="LineItem/Sku/Region"/>
            </UPSZONE>
            <ShipToName>
              <xsl:value-of select="ShippingAccount/Name"/>
            </ShipToName>
          </taCreateCustomerAddress>

          <xsl:if test="ShippingAccount/CustomerNumber!=$FinalDestinationAccount">
            <taCreateCustomerAddress>
              <CUSTNMBR>
                <xsl:value-of select = "MainAccount/CustomerNumber"/>
              </CUSTNMBR>
              <ADRSCODE>
                <xsl:text>S-</xsl:text>
                <xsl:value-of select = "FinalDestinationAccount/CustomerNumber"/>
              </ADRSCODE>
              <xsl:if test="LineItem/Sku/Region!=''">
                <xsl:choose>
                  <xsl:when test="LineItem/Sku/Region='North America'">
                    <TAXSCHID>
                      <xsl:text>AVATAX</xsl:text>
                    </TAXSCHID>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>
              <ADDRESS1>
                <xsl:value-of select = "FinalDestinationAccount/Address/AddressLine1"/>
              </ADDRESS1>
              <ADDRESS2>
                <xsl:value-of select = "FinalDestinationAccount/Address/AddressLine2"/>
              </ADDRESS2>
              <COUNTRY>
                <xsl:value-of select = "FinalDestinationAccount/Address/Country/CountryID"/>
              </COUNTRY>
              <CITY>
                <xsl:value-of select = "FinalDestinationAccount/Address/City"/>
              </CITY>
              <STATE>
                <xsl:value-of select = "FinalDestinationAccount/Address/State/StateID"/>
              </STATE>
              <ZIPCODE>
                <xsl:value-of select = "FinalDestinationAccount/Address/ZipCode"/>
              </ZIPCODE>
              <PHNUMBR1>
                <xsl:value-of select="normalize-space(translate(FinalDestinationAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
              </PHNUMBR1>
              <UPSZONE>
                <xsl:value-of select="LineItem/Sku/Region"/>
              </UPSZONE>
              <ShipToName>
                <xsl:value-of select="FinalDestinationAccount/Name"/>
              </ShipToName>
            </taCreateCustomerAddress>
          </xsl:if>

        </xsl:if>
        <xsl:if test="$IsCustomerExists = 'Yes'">
          <taCreateCustomerAddress>
            <CUSTNMBR>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CUSTNMBR>
            <ADRSCODE>
              <xsl:text>B-</xsl:text>
              <xsl:value-of select = "BillingAccount/CustomerNumber"/>
            </ADRSCODE>
            <xsl:if test="LineItem/Sku/Region!=''">
              <xsl:choose>
                <xsl:when test="LineItem/Sku/Region='North America'">
                  <TAXSCHID>
                    <xsl:text>AVATAX</xsl:text>
                  </TAXSCHID>
                </xsl:when>
              </xsl:choose>
            </xsl:if>
            <ADDRESS1>
              <xsl:value-of select = "BillingAccount/Address/AddressLine1"/>
            </ADDRESS1>
            <ADDRESS2>
              <xsl:value-of select = "BillingAccount/Address/AddressLine2"/>
            </ADDRESS2>
            <COUNTRY>
              <xsl:value-of select = "BillingAccount/Address/Country/CountryID"/>
            </COUNTRY>
            <CITY>
              <xsl:value-of select = "BillingAccount/Address/City"/>
            </CITY>
            <STATE>
              <xsl:value-of select = "BillingAccount/Address/State/StateID"/>
            </STATE>
            <ZIPCODE>
              <xsl:value-of select = "BillingAccount/Address/ZipCode"/>
            </ZIPCODE>
            <PHNUMBR1>
              <xsl:value-of select="normalize-space(translate(BillingAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
            </PHNUMBR1>
            <UPSZONE>
              <xsl:value-of select="LineItem/Sku/Region"/>
            </UPSZONE>
            <ShipToName>
              <xsl:value-of select="BillingAccount/Name"/>
            </ShipToName>
            <UpdateIfExists>1</UpdateIfExists>
          </taCreateCustomerAddress>
          <taCreateCustomerAddress>
            <CUSTNMBR>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CUSTNMBR>
            <ADRSCODE>
              <xsl:text>S-</xsl:text>
              <xsl:value-of select = "ShippingAccount/CustomerNumber"/>
            </ADRSCODE>
            <xsl:if test="LineItem/Sku/Region!=''">
              <xsl:choose>
                <xsl:when test="LineItem/Sku/Region='North America'">
                  <TAXSCHID>
                    <xsl:text>AVATAX</xsl:text>
                  </TAXSCHID>
                </xsl:when>
              </xsl:choose>
            </xsl:if>
            <ADDRESS1>
              <xsl:value-of select = "ShippingAccount/Address/AddressLine1"/>
            </ADDRESS1>
            <ADDRESS2>
              <xsl:value-of select = "ShippingAccount/Address/AddressLine2"/>
            </ADDRESS2>
            <COUNTRY>
              <xsl:value-of select = "ShippingAccount/Address/Country/CountryID"/>
            </COUNTRY>
            <CITY>
              <xsl:value-of select = "ShippingAccount/Address/City"/>
            </CITY>
            <STATE>
              <xsl:value-of select = "ShippingAccount/Address/State/StateID"/>
            </STATE>
            <ZIPCODE>
              <xsl:value-of select = "ShippingAccount/Address/ZipCode"/>
            </ZIPCODE>
            <PHNUMBR1>
              <xsl:value-of select="normalize-space(translate(ShippingAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
            </PHNUMBR1>
            <UPSZONE>
              <xsl:value-of select="LineItem/Sku/Region"/>
            </UPSZONE>
            <ShipToName>
              <xsl:value-of select="ShippingAccount/Name"/>
            </ShipToName>
            <UpdateIfExists>1</UpdateIfExists>
          </taCreateCustomerAddress>
          <xsl:if test="ShippingAccount/CustomerNumber!=$FinalDestinationAccount">
            <taCreateCustomerAddress>
              <CUSTNMBR>
                <xsl:value-of select = "MainAccount/CustomerNumber"/>
              </CUSTNMBR>
              <ADRSCODE>
                <xsl:text>S-</xsl:text>
                <xsl:value-of select = "FinalDestinationAccount/CustomerNumber"/>
              </ADRSCODE>
              <xsl:if test="LineItem/Sku/Region!=''">
                <xsl:choose>
                  <xsl:when test="LineItem/Sku/Region='North America'">
                    <TAXSCHID>
                      <xsl:text>AVATAX</xsl:text>
                    </TAXSCHID>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>
              <ADDRESS1>
                <xsl:value-of select = "FinalDestinationAccount/Address/AddressLine1"/>
              </ADDRESS1>
              <ADDRESS2>
                <xsl:value-of select = "FinalDestinationAccount/Address/AddressLine2"/>
              </ADDRESS2>
              <COUNTRY>
                <xsl:value-of select = "FinalDestinationAccount/Address/Country/CountryID"/>
              </COUNTRY>
              <CITY>
                <xsl:value-of select = "FinalDestinationAccount/Address/City"/>
              </CITY>
              <STATE>
                <xsl:value-of select = "FinalDestinationAccount/Address/State/StateID"/>
              </STATE>
              <ZIPCODE>
                <xsl:value-of select = "FinalDestinationAccount/Address/ZipCode"/>
              </ZIPCODE>
              <PHNUMBR1>
                <xsl:value-of select="normalize-space(translate(FinalDestinationAccount/PhoneNumber/MainPhone,'(;);-; ', ''))"/>
              </PHNUMBR1>
              <UPSZONE>
                <xsl:value-of select="LineItem/Sku/Region"/>
              </UPSZONE>
              <ShipToName>
                <xsl:value-of select="FinalDestinationAccount/Name"/>
              </ShipToName>
              <UpdateIfExists>1</UpdateIfExists>
            </taCreateCustomerAddress>
          </xsl:if>
          <!-- Tax Enhancement For NA -->
          <xsl:choose>
            <xsl:when test ="QuoteInformation/MainAccount/Region/Description='North America'">
              <taAccountTaxJurisdictionDetail_Items>
                <xsl:for-each select="QuoteInformation/MainAccount/CreditProfile/TaxDetail/TaxInformations">
                  <taAccountTaxJurisdictionDetail>
                    <CustomerID>
                      <xsl:value-of select = "//MainAccount/CustomerNumber"/>
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
                      <xsl:value-of select = "AuditInformation/CreatedBy"/>
                    </CreatedBy>
                    <ModifiedBy>
                      <xsl:value-of select = "AuditInformation/ModifiedBy"/>
                    </ModifiedBy>
                    <UpdateIfExists>1</UpdateIfExists>
                  </taAccountTaxJurisdictionDetail>
                </xsl:for-each>
              </taAccountTaxJurisdictionDetail_Items>
            </xsl:when>
          </xsl:choose>

          <!--Tax Enhancement For EU-->
          <xsl:choose>
            <xsl:when test ="//QuoteInformation/MainAccount/Region/Description='Europe'">
              <taAccountVATNumberReference_Items>
                <xsl:for-each select="//QuoteInformation/MainAccount/CreditProfile/TaxDetail/TaxInformations">
                  <taAccountVATNumberReference>
                    <CustomerID>
                      <xsl:value-of select = "//MainAccount/CustomerNumber"/>
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
                      <xsl:value-of select = "AuditInformation/CreatedBy"/>
                    </CreatedBy>
                    <ModifiedBy>
                      <xsl:value-of select = "AuditInformation/ModifiedBy"/>
                    </ModifiedBy>
                    <UpdateIfExists>1</UpdateIfExists>
                  </taAccountVATNumberReference>
                </xsl:for-each>
              </taAccountVATNumberReference_Items>
            </xsl:when>
          </xsl:choose>
        </xsl:if>

        <taAddressReference_Items>
          <taAddressReference>
            <CustomerID>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CustomerID>
            <AccountAddressTypeId>1</AccountAddressTypeId>
            <AccountAddressNumber>
              <xsl:value-of select = "BillingAccount/CustomerNumber"/>
            </AccountAddressNumber>
            <QuoteId>
              <xsl:value-of select = "QuoteId"/>
            </QuoteId>

            <ModifiedUser>
              <xsl:value-of select = "AuditInformation/ModifiedBy"/>
            </ModifiedUser>
          </taAddressReference>
          <taAddressReference>
            <CustomerID>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </CustomerID>
            <AccountAddressTypeId>2</AccountAddressTypeId>
            <AccountAddressNumber>
              <xsl:value-of select = "ShippingAccount/CustomerNumber"/>
            </AccountAddressNumber>
            <QuoteId>
              <xsl:value-of select = "QuoteId"/>
            </QuoteId>
            <ModifiedUser>
              <xsl:value-of select = "AuditInformation/ModifiedBy"/>
            </ModifiedUser>
          </taAddressReference>
        </taAddressReference_Items>

        <taQuoteMaster_Items>
          <taQuoteMaster>
            <QuoteNumber>
              <xsl:value-of select = "QuoteNumber"/>
            </QuoteNumber>
            <QuoteID>
              <xsl:value-of select = "QuoteId"/>
            </QuoteID>
            <QuoteName>
              <xsl:value-of select = "QuoteName"/>
            </QuoteName>
            <RevisionNumber>
              <xsl:value-of select = "RevisionNumber"/>
            </RevisionNumber>
            <MainCustomerNumber>
              <xsl:value-of select = "MainAccount/CustomerNumber"/>
            </MainCustomerNumber>
            <BillToCustomerNumber>
              <xsl:value-of select = "BillingAccount/CustomerNumber"/>
            </BillToCustomerNumber>
            <ShipToCustomerNumber>
              <xsl:value-of select = "ShippingAccount/CustomerNumber"/>
            </ShipToCustomerNumber>
            <FinalDestCustomerNumber>
              <xsl:value-of select = "FinalDestinationAccount/CustomerNumber"/>
            </FinalDestCustomerNumber>
            <WarehouseNumber>
              <xsl:value-of select = "ShipFromAccount/WarehouseNumber"/>
            </WarehouseNumber>
            <StatusCode>
              <xsl:value-of select = "Status/StatusCode"/>
            </StatusCode>
            <Currency>
              <xsl:value-of select = "Currency"/>
            </Currency>
            <ItemNumber>
              <xsl:value-of select = "LineItem/Sku/SkuNumber"/>
            </ItemNumber>
            <Quantity>
              <xsl:value-of select = "LineItem/Quantity"/>
            </Quantity>
            <PricePerSku>
              <xsl:value-of select = "LineItem/PricePerSku"/>
            </PricePerSku>

            <SubStatus>
              <xsl:value-of select ="Status/StatusReason"/>
            </SubStatus>

            <IsFIP>
              <xsl:value-of select = "ShippingInformation/IsFIP"/>
            </IsFIP>
            <FreightPerUnit>
              <xsl:value-of select = "ShippingInformation/FreightPerUnit"/>
            </FreightPerUnit>
            <ModifiedBy>
              <xsl:value-of select = "AuditInformation/ModifiedBy"/>
            </ModifiedBy>
            <ModifiedOn>
              <xsl:value-of select = "AuditInformation/ModifiedDate"/>
            </ModifiedOn>
            <CreatedBy>
              <xsl:value-of select = "AuditInformation/CreatedBy"/>
            </CreatedBy>
            <CreatedOn>
              <xsl:value-of select = "AuditInformation/CreatedDate"/>
            </CreatedOn>
          </taQuoteMaster>
        </taQuoteMaster_Items>

      </RMCustomerMasterType>
    </eConnect>
  </xsl:template>
</xsl:stylesheet>