   M S B A M L   `   `   `  ���� H  DToolSyncOrder, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ToolSyncOrderToolSyncOrder      ToolSyncOrder.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:ToolSyncOrder     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -   $
��1024��$	��768��  � 	Resources  &    ToolSyncOrder.RowNumberConverter   RowNumberConverter��%   &	          .���  � ColumnDefinitions �� $��2* ��� $��1* �.+��  � RowDefinitions �� $
��Auto ��� $��* �.+��  �� $
��Auto ��� .+��� $��10q� �� Orientation$ 
Horizontal=�.+��� -   $��80��$��40��$��Browse��� $��1��$��10q� �� �� $
��Auto �.+��� -   $��1��$��80��$��40��$��Right=�
������$��
Sync order��� $��1�� �� $
��Auto ��� .+��� $��15,0,0,0q�$ 
Horizontal=�.+��� $��13(�$
��Bold"�$��#���� -    ��Name$ NumberOrderTextBlock��$��13(�$
��Bold"��� -   $��logListView��$��1��$
��10,0q�$��15(����� *
LogEntries+ �� View �� .P���  �� Width$
 Auto��$��Status�� �� DisplayMemberBinding �� *OrderStatus+�� $
 Auto��$��Order No�� �� *	OrderNo+�� $
 Auto��$��Order ID�� �� *	OrderId+�� $
 Auto��$��Shift ID�� �� *	ShiftId+����  �� 
TargetType" ��	 �� Triggers	 �� 
 �� Binding
 �� *IsVoidOrder+ �� Value True.a���  �� Property � �� �� Value
 ������� 
 �� *IsSuccessful+ True.a���  � ��
 ������  � ��
 � � ���  � ��$ 1q�^�  ^� 
Conditions ��  �� Binding �� *IsVoidOrder+ �� Value True��  �� *IsSuccessful+ True.0���  � �� �#83C760��  � ��
 � � ���  � ��$ 1q�� $��1��$��10q� �� $
��Auto ��� .+��� 
��� ���
���   �$��2q�._��� $��5,0,0,0q�$��18(�$
��Bold"�$��Filter by Order ID:���� -   $��orderIdFilterTextBox��$��1��$��0,5,0,0q� �� AcceptsReturn .  �� 
AcceptsTab . 