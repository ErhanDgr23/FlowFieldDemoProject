# 🧭 FlowField Pathfinding (2D Tilemap)

Basit bir **FlowField pathfinding sistemi** için geliştirilmiş bir Unity script koleksiyonudur.  
Biraz acemice yazılmış kısımlar olabilir — amacı öğrenme ve pratik yapma. Yine de temel işlevleri çalışır durumdadır.

---

## 🎯 Kısa Tanım

Bu proje, **2D top-down** oyunlarda kullanılan, tilemap tabanlı bir FlowField akış alanı (flow field) oluşturur ve ajanların (FlowAgent) engellerin etrafından dolaşarak hedefe doğru akıcı şekilde ilerlemesini sağlar. Sistem **Tilemap** gerektirir ve **Rectangle** Tilemap tipi için test edilmiştir.

---

## ⚙️ Özellikler

- 🧩 2D Top-Down + Tilemap uyumlu (Rectangle Tilemap ile testli)  
- 🚧 Engellerin kapladığı hücreleri `FlowFieldObstacle` ile belirleyip TileMap üzerine otomatik olarak yerleştirme  
- 🔄 Dinamik alan oluşturma: Oyun içinde flowfield yeniden hesaplanabilir  
- 🧍‍♂️ FlowAgent entegrasyonu: Ajanlar en yakın flow alanına ışınlanıp akışı takip eder  
- 🎯 TileCost (ScriptableObject) desteği: Layer/Tile bazlı hareket maliyetleri belirlenebilir

---

## ⚠️ Önemli Uyarılar / Kısıtlamalar

- ⚠️ **Sadece 2D Top-Down** oyunlar için tasarlanmıştır.  
- ⚠️ **Tilemap zorunludur** — Tilemap olmadan çalışmaz.  
- ⚠️ **Sadece Rectangle Tilemap** ile test edilmiştir; diğer Tilemap türlerinde beklenmeyen sonuçlar olabilir.  
- ⚠️ **Kodda bazı hatalar veya eksiklikler olabilir.** Bu proje öğrenme amaçlı olduğundan, hatalarla karşılaşırsanız lütfen issue açın veya PR gönderin.  
- ⚠️ Performans kritik projelerde sistemin optimizasyonuna dikkat edilmelidir (büyük haritalarda spawn tarama vb. maliyetli olabilir).

---

## 🧠 Nasıl Çalışır?

1. **FlowFieldObstacle** component’i ile sahnedeki engeller, `Tilemap` ve `TileCost (ScriptableObject)` bilgileri tanımlanır.  
2. Engel objelerine `FlowFieldObstacle.cs` eklenir ve **tileCoverage** üzerinden manuel olarak kapladığı hücre alanları belirlenir.  
3. **FlowField Generator**, bu verileri ve **TileCostSO** içindeki layer bilgilerini kullanarak hedef objeye doğru dinamik bir **akış alanı (Flow Field)** oluşturur.  
4. **FlowAgent**, bu akış alanını okuyarak en uygun yönü belirler ve akıcı bir şekilde hedefe doğru ilerler.  

> Bu yapı sayesinde engeller ve hareket maliyetleri (cost) gerçek zamanlı olarak Tilemap üzerinde güncellenebilir; yani oyun içinde alan tekrar oluşturulabilir.

---

## 🧩 Kullanım (Hızlı Başlangıç)

1. Unity'de yeni bir **Tilemap (Rectangular)** oluştur.  
2. `FlowFieldObstacle.cs` script’ini sahnedeki engel objelerine ekle.  
3. `tileCoverage` dizisini düzenleyerek objenin kapladığı alanı manuel olarak belirt. (float değerler world offset olarak verilebilir, sistem en yakın tile'ı bulur)  
4. `targetTilemap` ve `obstacleTile` alanlarını inspector'dan atayın.  
5. `TileCostSO` (ScriptableObject) oluşturup layer bazlı maliyetleri tanımlayın (ör: zemin=1, çamur=2, su=999).  
6. FlowField Generator ve FlowAgent prefabriklerini sahneye ekleyin (projede örnek sahne/Prefab bulunuyorsa kullanın).  
7. Oyunu başlatın — sistem engel verilerine göre Tilemap'i günceller ve flowfield üretir.

---