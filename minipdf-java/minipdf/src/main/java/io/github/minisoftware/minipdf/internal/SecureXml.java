package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.MiniPdfException;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import java.io.ByteArrayInputStream;

public final class SecureXml {
    private SecureXml() {
    }

    public static XMLStreamReader reader(byte[] xml) throws MiniPdfException {
        XMLInputFactory factory = XMLInputFactory.newFactory();
        factory.setProperty(XMLInputFactory.SUPPORT_DTD, false);
        factory.setProperty(XMLInputFactory.IS_SUPPORTING_EXTERNAL_ENTITIES, false);
        try {
            return factory.createXMLStreamReader(new ByteArrayInputStream(xml));
        } catch (XMLStreamException exception) {
            throw parseError(exception);
        }
    }

    public static MiniPdfException parseError(XMLStreamException exception) {
        return new MiniPdfException(
                MiniPdfException.Kind.XML_PARSE,
                "XML parse error: " + exception.getMessage(),
                exception);
    }
}