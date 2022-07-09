<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<ShipOrder>
			<Header>
				<DocID>
					<xsl:value-of select="SOPDetails/SOP/DocID"/>
				</DocID>
				<DocDateTime>
					<xsl:value-of select="SOPDetails/SOP/DocDateTime"/>
				</DocDateTime>
				<FromPartnerInfo>
					<xsl:attribute name="Agency"><xsl:value-of select="SOPDetails/SOP/FromAgency"/></xsl:attribute>
					<FromPartnerID>
						<xsl:value-of select="SOPDetails/SOP/FromID"/>
					</FromPartnerID>
				</FromPartnerInfo> 
				<ToPartnerInfo>
					<xsl:attribute name="Agency"><xsl:value-of select="SOPDetails/SOP/ToAgency"/></xsl:attribute>
					<ToPartnerID>
						<xsl:value-of select="SOPDetails/SOP/ToID"/>
					</ToPartnerID>
				</ToPartnerInfo>
			</Header>
			<Order>
				<OrderNo>
					<xsl:value-of select="SOPDetails/SOP/OrderNo"/>
				</OrderNo>
				<OrderStatus>
					<xsl:value-of select="SOPDetails/SOP/OrderStatus"/>
				</OrderStatus>
				<Reference1>
					<xsl:value-of select="SOPDetails/SOP/InvoiceNo"/>
				</Reference1>
				<Comment>
					<xsl:value-of select="normalize-space(SOPDetails/SOP/Comment)"/>
				</Comment>
				<Comment1>
					<xsl:value-of select="SOPDetails/SOP/Comment1"/>
				</Comment1>
				<Comment2>
					<xsl:value-of select="SOPDetails/SOP/Comment2"/>
				</Comment2>
				<Comment3>
					<xsl:value-of select="SOPDetails/SOP/Comment3"/>
				</Comment3>
				<Comment4>
					<xsl:value-of select="SOPDetails/SOP/Comment4"/>
				</Comment4>
				<xsl:for-each select="SOPDetails/SOP">
					<OrderLine>
			            <HazmatFlag>
			              <xsl:value-of select ="Hazmatflag"/>
			            </HazmatFlag>
                      <HazmatClass>
                        <xsl:value-of select ="hazmatclass"/>
                      </HazmatClass>
                      <UnNumber>
                        <xsl:value-of select ="unnumber"/>
                      </UnNumber>
                      <PackageGroup>
                        <xsl:value-of select ="packagegroup"/>
                      </PackageGroup>
                      <TechnicalName>
                        <xsl:value-of select ="technicalname"/>
                      </TechnicalName>
                      <PhoneNumber>
                        <xsl:value-of select ="phonenumber"/>
                      </PhoneNumber>
			            <TemperatureControl>
			              <xsl:value-of select ="TemperatureControl"/>
			            </TemperatureControl>
			            <FreezeProtect>
			              <xsl:value-of select ="FreezeProtect"/>
			            </FreezeProtect>
			            <Weight>
			              <xsl:value-of select ="Weight"/>
			            </Weight>
			            <FoodGradeFlag>
			              <xsl:value-of select ="FoodGradeFlag"/>
			            </FoodGradeFlag>
			            <UOM>
			              <xsl:value-of select ="UOM"/>
			            </UOM>
						<LineNumber>
						<!--<xsl:value-of select="SOPDetails/SOP/LineID"/>-->
							<xsl:value-of select="LineID"/>
						</LineNumber>
						<LineStatus>
							<xsl:value-of select="LineStatus"/>
						</LineStatus>
						<LineReference1>
							<xsl:value-of select="VendorItemID"/>
						</LineReference1>
						<LineReference2>
							<xsl:value-of select="LineRef1"/>
						</LineReference2>
						<LineReference3>
							<xsl:value-of select="concat(LineRef2,LineRef3)"/>
						</LineReference3>
						<ItemNumber>
							<xsl:value-of select="ItemNumber"/>
						</ItemNumber>
						<ItemDesc>
							<xsl:value-of select="ItemDesc"/>
						</ItemDesc>
						<ShipQty>
							<xsl:value-of select="QTYToInvoice"/>
						</ShipQty>
						<CurrencyCode>
							<xsl:value-of select="CurrencyID"/>
						</CurrencyCode>
						<LineComment>
							<xsl:value-of select="normalize-space(LineComment)"/>
						</LineComment>
						<LineComment1>
							<xsl:value-of select="LineComment1"/>
						</LineComment1>
						<LineComment2>
							<xsl:value-of select="LineComment2"/>
						</LineComment2>
						<LineComment3>
							<xsl:value-of select="LineComment3"/>
						</LineComment3>
						<LineComment4>
							<xsl:value-of select="LineComment4"/>
						</LineComment4>
            <OrderDate>
              <xsl:value-of select="DocDateTime"/>
            </OrderDate>
						<RequestedShipDate>
							<xsl:value-of select="ReqShipDate"/>
						</RequestedShipDate>
						<RequestedDeliveryDate>
							<xsl:value-of select="ReqDeliveryDate"/>
						</RequestedDeliveryDate>
						<FreightOption>
							<CarrierCode>
								<xsl:value-of select="CarrierCode"/>
							</CarrierCode>
							<ServiceCode>
								<xsl:value-of select="ServiceCode"/>
							</ServiceCode>
							<Carrier>
								<xsl:value-of select="Carrier"/>
							</Carrier>
							<Carrier_name>
								<xsl:value-of select="CarrierName"/>
							</Carrier_name>
							<Carrier_Account_Number>
								<xsl:value-of select="CarrierAccNo"/>
							</Carrier_Account_Number>
							<carrier_phone_number>
								<xsl:value-of select="CarrierPhNum"/>
							</carrier_phone_number>
							<carrier_address>
								<xsl:value-of select="CarrierAddr"/>
							</carrier_address>
							<BillingAccount>
								<xsl:value-of select="BillingAccount"/>
							</BillingAccount>
							<FreightTerm>
								<xsl:value-of select="BillToParty"/>
							</FreightTerm>
						</FreightOption>
						<ShipTo>
							<ShipToID>
								<xsl:value-of select="ShipToID"/>
							</ShipToID>
							<ShipToAttn>
								<xsl:value-of select="ContactName"/>
							</ShipToAttn>
							<ShipToCompanyName>
								<xsl:value-of select="ShipToName"/>
							</ShipToCompanyName>
							<ShipToAddress1>
								<xsl:value-of select="Address1"/>
							</ShipToAddress1>
							<ShipToAddress2>
								<xsl:value-of select="Address2"/>
							</ShipToAddress2>
							<ShipToAddress3>
								<xsl:value-of select="Address3"/>
							</ShipToAddress3>
							<ShipToCity>
								<xsl:value-of select="City"/>
							</ShipToCity>
							<ShipToStateProvince>
								<xsl:value-of select="State"/>
							</ShipToStateProvince>
							<ShipToPostalCode>
								<xsl:value-of select="ZipCode"/>
							</ShipToPostalCode>
							<ShipToCountry>
								<xsl:value-of select="Country"/>
							</ShipToCountry>
							<ShipToPhone1>
								<xsl:value-of select="Phone1"/>
							</ShipToPhone1>
							<ShipToPhone2>
								<xsl:value-of select="Phone2"/>
							</ShipToPhone2>
							<ShipToEmail>
								<xsl:value-of select="CustomerEmail"/>
							</ShipToEmail>
							<ShipToMainPhone1>
								<xsl:value-of select="CustMainPhone1"/>
							</ShipToMainPhone1>
							<ShipToMainPhone2>
								<xsl:value-of select="CustMainPhone2"/>
							</ShipToMainPhone2>
						</ShipTo>
						<ShipFrom>
							<ShipFromID>
								<xsl:value-of select="LocationCode"/>
							</ShipFromID>
						</ShipFrom>
						<GroupBy>
							<GroupKey1><xsl:value-of select="GroupKey1"></xsl:value-of></GroupKey1>
							<GroupKey2><xsl:value-of select="GroupKey2"></xsl:value-of></GroupKey2>
							<GroupKey3><xsl:value-of select="GroupKey3"></xsl:value-of></GroupKey3>
						</GroupBy>
					</OrderLine>
				</xsl:for-each>
				<Option>
						<OptionName>PONumber</OptionName>
						<OptionValue><xsl:value-of select="SOPDetails/SOP/CustPONumber"/></OptionValue>
				</Option>
			</Order>
		</ShipOrder>
	</xsl:template>
</xsl:stylesheet>